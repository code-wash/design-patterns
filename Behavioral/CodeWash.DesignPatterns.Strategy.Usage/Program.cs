using CodeWash.DesignPatterns.Strategy.ClaimSettlement;
using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;
using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Downstream;
using CodeWash.DesignPatterns.Strategy.Usage;
using Microsoft.Extensions.DependencyInjection;

// Wire it the way a real ASP.NET app would: strategies registered against one
// interface, and nothing downstream naming a concrete type.
var services = new ServiceCollection();

// The strategies take ILogger<T> exactly as production code would; the demo
// silences it so the output stays deterministic. Add AddSimpleConsole() to see
// the real log lines.
services.AddLogging();

// Stand-ins for the downstream systems. Their real implementations are where
// the domain complexity lives, and none of it is needed to see the pattern.
services.AddSingleton<StubDownstream>();
services.AddSingleton<ICardSecurityService>(sp => sp.GetRequiredService<StubDownstream>());
services.AddSingleton<IFraudExposureAssessor>(sp => sp.GetRequiredService<StubDownstream>());
services.AddSingleton<ILossProvisionLedger>(sp => sp.GetRequiredService<StubDownstream>());
services.AddSingleton<IMerchantRecoveryClient>(sp => sp.GetRequiredService<StubDownstream>());

services.AddClaimSettlement();

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

// The only object an endpoint would hold. It has no idea how many claim types
// exist, nor which downstream systems any of them touch.
var settlement = scope.ServiceProvider.GetRequiredService<ClaimSettlementService>();

var today = DateOnly.FromDateTime(DateTime.UtcNow);

SettlementRequest[] requests =
[
    // Fraud reported promptly -> card blocked, loss provisioned.
    new()
    {
        ClaimId = "CLM-1001",
        ClaimType = ClaimType.Fraud,
        CardReference = "card-77",
        TransactionAmount = 249.00m,
        TransactionDate = today.AddDays(-10),
        ReportedOn = today.AddDays(-2),
    },

    // Fraud reported far too late -> rejected on the reporting window.
    new()
    {
        ClaimId = "CLM-1002",
        ClaimType = ClaimType.Fraud,
        CardReference = "card-77",
        TransactionAmount = 80.00m,
        TransactionDate = today.AddDays(-200),
        ReportedOn = today,
    },

    // Dispute before the merchant has replied -> deferred, nothing settles.
    new()
    {
        ClaimId = "CLM-2001",
        ClaimType = ClaimType.Dispute,
        CardReference = "card-91",
        TransactionAmount = 500.00m,
        ClaimedAmount = 120.00m,
        TransactionDate = today.AddDays(-14),
        ReasonCode = "13.1",
        MerchantResponseWindowClosed = false,
    },

    // Same dispute once the window has closed -> partial chargeback.
    new()
    {
        ClaimId = "CLM-2002",
        ClaimType = ClaimType.Dispute,
        CardReference = "card-91",
        TransactionAmount = 500.00m,
        ClaimedAmount = 120.00m,
        TransactionDate = today.AddDays(-45),
        ReasonCode = "13.1",
        MerchantResponseWindowClosed = true,
    },

    // Dispute missing its reason code -> rejected by a rule fraud does not have.
    new()
    {
        ClaimId = "CLM-2003",
        ClaimType = ClaimType.Dispute,
        CardReference = "card-91",
        TransactionAmount = 75.00m,
        TransactionDate = today.AddDays(-3),
        MerchantResponseWindowClosed = true,
    },
];

foreach (var request in requests)
{
    var decision = await settlement.SettleAsync(request);

    Console.WriteLine($"  {request.ClaimType,-14} {decision.Outcome,-9} {decision.SettledAmount,8:0.00}  {decision.Reason}");
    Console.WriteLine();
}
