namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

public enum SettlementOutcome
{
    /// <summary>Money moved.</summary>
    Settled,

    /// <summary>Valid, but cannot be settled yet.</summary>
    Deferred,

    /// <summary>Failed the rules for its claim type.</summary>
    Rejected,
}
