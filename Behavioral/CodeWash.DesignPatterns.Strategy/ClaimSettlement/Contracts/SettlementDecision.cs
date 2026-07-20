namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;

/// <summary>
/// One result model for every claim type, matching
/// <see cref="SettlementRequest"/>. Same contract in, same contract out,
/// different algorithm between -- the definition of interchangeable algorithms.
/// </summary>
public sealed record SettlementDecision
{
    public required string ClaimId { get; init; }

    public required SettlementOutcome Outcome { get; init; }

    public decimal SettledAmount { get; init; }

    /// <summary>Which route the money took. Differs sharply per claim type.</summary>
    public string? RecoveryRoute { get; init; }

    public required string Reason { get; init; }

    public static SettlementDecision Rejected(string claimId, string reason) =>
        new() { ClaimId = claimId, Outcome = SettlementOutcome.Rejected, Reason = reason };

    public static SettlementDecision Deferred(string claimId, string reason) =>
        new() { ClaimId = claimId, Outcome = SettlementOutcome.Deferred, Reason = reason };

    public static SettlementDecision Settled(string claimId, decimal amount, string route, string reason) =>
        new()
        {
            ClaimId = claimId,
            Outcome = SettlementOutcome.Settled,
            SettledAmount = amount,
            RecoveryRoute = route,
            Reason = reason,
        };
}
