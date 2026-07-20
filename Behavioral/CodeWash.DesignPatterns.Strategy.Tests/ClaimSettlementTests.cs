using CodeWash.DesignPatterns.Strategy.ClaimSettlement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeWash.DesignPatterns.Strategy.Tests;

/// <summary>
/// These tests demonstrate the payoff of Strategy: identical input, identical
/// output contract, genuinely different algorithm -- and a context that never
/// learns how many claim types exist.
/// </summary>
public class ClaimSettlementTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static SettlementRequest FraudClaim(Action<RequestBuilder>? customise = null)
    {
        var builder = new RequestBuilder
        {
            ClaimType = ClaimType.Fraud,
            ReportedOn = Today.AddDays(-1),
        };
        customise?.Invoke(builder);
        return builder.Build();
    }

    private static SettlementRequest DisputeClaim(Action<RequestBuilder>? customise = null)
    {
        var builder = new RequestBuilder
        {
            ClaimType = ClaimType.Dispute,
            ReasonCode = "13.1",
            MerchantResponseWindowClosed = true,
        };
        customise?.Invoke(builder);
        return builder.Build();
    }

    private static ClaimSettlementService BuildService(out SpyDownstream downstream)
    {
        var spy = new SpyDownstream();

        var services = new ServiceCollection();
        services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<ICardSecurityService>(spy);
        services.AddSingleton<IFraudExposureAssessor>(spy);
        services.AddSingleton<ILossProvisionLedger>(spy);
        services.AddSingleton<IMerchantRecoveryClient>(spy);
        services.AddClaimSettlement();

        downstream = spy;
        return services.BuildServiceProvider().GetRequiredService<ClaimSettlementService>();
    }

    // -- The core claim: same request shape, different algorithm -------------

    [Fact]
    public async Task SameRequestShape_TakesADifferentRoute_PerClaimType()
    {
        var service = BuildService(out _);

        var fraud = await service.SettleAsync(FraudClaim());
        var dispute = await service.SettleAsync(DisputeClaim());

        Assert.Equal(SettlementOutcome.Settled, fraud.Outcome);
        Assert.Equal(SettlementOutcome.Settled, dispute.Outcome);

        // The bank absorbs fraud; the merchant is billed for disputes.
        Assert.Equal("issuer-loss-provision", fraud.RecoveryRoute);
        Assert.Equal("merchant-chargeback", dispute.RecoveryRoute);
    }

    [Fact]
    public async Task TheTwoStrategies_TouchEntirelyDifferentDownstreamSystems()
    {
        // This is the structural argument for Strategy in this example: the
        // algorithms do not merely differ in thresholds, they call different
        // services in a different order.
        var service = BuildService(out var downstream);

        await service.SettleAsync(FraudClaim());

        Assert.Equal(["block-card", "assess-exposure", "provision-loss"], downstream.Calls);

        downstream.Calls.Clear();
        await service.SettleAsync(DisputeClaim());

        Assert.Equal(["raise-chargeback"], downstream.Calls);
    }

    [Fact]
    public async Task Fraud_BlocksTheCard_WhereasADisputeNeverDoes()
    {
        // The card was used legitimately in a dispute; blocking it would be wrong.
        var service = BuildService(out var downstream);

        await service.SettleAsync(DisputeClaim());
        Assert.DoesNotContain("block-card", downstream.Calls);

        await service.SettleAsync(FraudClaim());
        Assert.Contains("block-card", downstream.Calls);
    }

    [Fact]
    public async Task Dispute_DefersUntilTheMerchantHasReplied_WhereasFraudNeverWaits()
    {
        var service = BuildService(out var downstream);

        var dispute = await service.SettleAsync(DisputeClaim(r => r.MerchantResponseWindowClosed = false));
        Assert.Equal(SettlementOutcome.Deferred, dispute.Outcome);
        Assert.Empty(downstream.Calls);

        var fraud = await service.SettleAsync(FraudClaim());
        Assert.Equal(SettlementOutcome.Settled, fraud.Outcome);
    }

    [Fact]
    public async Task Fraud_DefersForManualReview_ForItsOwnQuiteDifferentReason()
    {
        // Both types can defer -- but never for the same cause.
        var service = BuildService(out var downstream);
        downstream.RequiresManualReview = true;

        var fraud = await service.SettleAsync(FraudClaim());

        Assert.Equal(SettlementOutcome.Deferred, fraud.Outcome);
        Assert.Contains("manual review", fraud.Reason);

        // The card is still blocked: containment happens before the decision.
        Assert.Contains("block-card", downstream.Calls);
        Assert.DoesNotContain("provision-loss", downstream.Calls);
    }

    [Fact]
    public async Task Dispute_AllowsPartialAmounts_ButFraudDoesNot()
    {
        var service = BuildService(out _);

        var dispute = await service.SettleAsync(DisputeClaim(r =>
        {
            r.TransactionAmount = 500m;
            r.ClaimedAmount = 120m;
        }));

        var fraud = await service.SettleAsync(FraudClaim(r =>
        {
            r.TransactionAmount = 500m;
            r.ClaimedAmount = 120m;
        }));

        Assert.Equal(SettlementOutcome.Settled, dispute.Outcome);
        Assert.Equal(120m, dispute.SettledAmount);

        Assert.Equal(SettlementOutcome.Rejected, fraud.Outcome);
        Assert.Contains("full amount", fraud.Reason);
    }

    [Fact]
    public async Task ValidationRulesAreClaimTypeSpecific()
    {
        var service = BuildService(out _);

        // A missing reason code sinks a dispute...
        var dispute = await service.SettleAsync(DisputeClaim(r => r.ReasonCode = null));
        Assert.Equal(SettlementOutcome.Rejected, dispute.Outcome);
        Assert.Contains("reason code", dispute.Reason);

        // ...but fraud has no such concept and settles fine without one.
        var fraud = await service.SettleAsync(FraudClaim(r => r.ReasonCode = null));
        Assert.Equal(SettlementOutcome.Settled, fraud.Outcome);
    }

    [Fact]
    public async Task ReportingWindows_DifferBetweenClaimTypes()
    {
        var service = BuildService(out _);

        // 90 days: past the 60-day fraud window, inside the 120-day chargeback window.
        var transactionDate = Today.AddDays(-90);

        var fraud = await service.SettleAsync(FraudClaim(r =>
        {
            r.TransactionDate = transactionDate;
            r.ReportedOn = Today;
        }));

        var dispute = await service.SettleAsync(DisputeClaim(r => r.TransactionDate = transactionDate));

        Assert.Equal(SettlementOutcome.Rejected, fraud.Outcome);
        Assert.Equal(SettlementOutcome.Settled, dispute.Outcome);
    }

    // -- The structural claim: the context does not know the claim types -----

    [Fact]
    public void EveryClaimType_HasARegisteredStrategy()
    {
        var services = new ServiceCollection();
        var spy = new SpyDownstream();
        services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<ICardSecurityService>(spy);
        services.AddSingleton<IFraudExposureAssessor>(spy);
        services.AddSingleton<ILossProvisionLedger>(spy);
        services.AddSingleton<IMerchantRecoveryClient>(spy);
        services.AddClaimSettlement();

        using var provider = services.BuildServiceProvider();
        var selector = provider.GetRequiredService<ISettlementStrategySelector>();

        // Walking the enum rather than naming strategies: adding a claim type
        // without registering a strategy fails here, not in production.
        foreach (var claimType in Enum.GetValues<ClaimType>())
        {
            Assert.Equal(claimType, selector.SelectFor(claimType).Handles);
        }
    }

    [Fact]
    public void Selector_FailsFast_WhenTwoStrategiesClaimTheSameType()
    {
        ISettlementStrategy[] duplicates = [new StubStrategy(ClaimType.Fraud), new StubStrategy(ClaimType.Fraud)];

        var exception = Assert.Throws<InvalidOperationException>(() => new SettlementStrategySelector(duplicates));
        Assert.Contains(nameof(ClaimType.Fraud), exception.Message);
    }

    [Fact]
    public void Selector_ThrowsClearly_ForAnUnregisteredClaimType()
    {
        var selector = new SettlementStrategySelector([new StubStrategy(ClaimType.Fraud)]);

        var exception = Assert.Throws<NotSupportedException>(() => selector.SelectFor(ClaimType.Dispute));
        Assert.Contains(nameof(ClaimType.Dispute), exception.Message);
    }

    // -- Test doubles --------------------------------------------------------

    private sealed class RequestBuilder
    {
        public string ClaimId { get; set; } = "CLM-0001";

        public ClaimType ClaimType { get; set; }

        public string CardReference { get; set; } = "card-1";

        public decimal TransactionAmount { get; set; } = 100m;

        public decimal? ClaimedAmount { get; set; }

        public DateOnly TransactionDate { get; set; } = Today.AddDays(-5);

        public DateOnly? ReportedOn { get; set; }

        public string? ReasonCode { get; set; }

        public bool MerchantResponseWindowClosed { get; set; }

        public SettlementRequest Build() => new()
        {
            ClaimId = ClaimId,
            ClaimType = ClaimType,
            CardReference = CardReference,
            TransactionAmount = TransactionAmount,
            ClaimedAmount = ClaimedAmount,
            TransactionDate = TransactionDate,
            ReportedOn = ReportedOn,
            ReasonCode = ReasonCode,
            MerchantResponseWindowClosed = MerchantResponseWindowClosed,
        };
    }

    /// <summary>Records which downstream systems were called, and in what order.</summary>
    private sealed class SpyDownstream
        : ICardSecurityService, IFraudExposureAssessor, ILossProvisionLedger, IMerchantRecoveryClient
    {
        public List<string> Calls { get; } = [];

        public bool RequiresManualReview { get; set; }

        public Task BlockCardAsync(string cardReference, CancellationToken cancellationToken = default)
        {
            Calls.Add("block-card");
            return Task.CompletedTask;
        }

        public Task<FraudExposure> AssessAsync(SettlementRequest request, CancellationToken cancellationToken = default)
        {
            Calls.Add("assess-exposure");
            return Task.FromResult(new FraudExposure
            {
                RequiresManualReview = RequiresManualReview,
                RecoverableAmount = request.TransactionAmount,
            });
        }

        public Task ProvisionAsync(string claimId, decimal amount, CancellationToken cancellationToken = default)
        {
            Calls.Add("provision-loss");
            return Task.CompletedTask;
        }

        public Task RaiseChargebackAsync(
            string claimId, decimal amount, string reasonCode, CancellationToken cancellationToken = default)
        {
            Calls.Add("raise-chargeback");
            return Task.CompletedTask;
        }
    }

    private sealed class StubStrategy(ClaimType handles) : ISettlementStrategy
    {
        public ClaimType Handles { get; } = handles;

        public Task<SettlementDecision> SettleAsync(
            SettlementRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(SettlementDecision.Settled(request.ClaimId, 0m, "stub", "stub"));
    }
}
