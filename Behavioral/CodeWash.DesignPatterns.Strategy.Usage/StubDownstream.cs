using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;
using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Downstream;

namespace CodeWash.DesignPatterns.Strategy.Usage;

/// <summary>
/// One stub standing in for every downstream system, so the demo stays
/// about the pattern rather than about card schemes.
/// </summary>
internal sealed class StubDownstream
    : ICardSecurityService, IFraudExposureAssessor, ILossProvisionLedger, IMerchantRecoveryClient
{
    public Task BlockCardAsync(string cardReference, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"  [security] blocked {cardReference}");
        return Task.CompletedTask;
    }

    public Task<FraudExposure> AssessAsync(SettlementRequest request, CancellationToken cancellationToken = default)
    {
        // A real assessor weighs many signals. Here: large claims get a human.
        var exposure = new FraudExposure
        {
            RequiresManualReview = request.TransactionAmount > 1_000m,
            RecoverableAmount = request.TransactionAmount,
        };

        return Task.FromResult(exposure);
    }

    public Task ProvisionAsync(string claimId, decimal amount, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"  [ledger]   provisioned {amount:0.00} for {claimId}");
        return Task.CompletedTask;
    }

    public Task RaiseChargebackAsync(
        string claimId, decimal amount, string reasonCode, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"  [merchant] chargeback {amount:0.00} for {claimId} ({reasonCode})");
        return Task.CompletedTask;
    }
}
