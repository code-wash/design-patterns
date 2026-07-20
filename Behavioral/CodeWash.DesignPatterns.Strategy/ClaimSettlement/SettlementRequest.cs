namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// One request model for every claim type -- this shared contract is the
/// constraint that makes Strategy the right shape.
/// </summary>
public sealed record SettlementRequest
{
    public required string ClaimId { get; init; }

    public required ClaimType ClaimType { get; init; }

    public required string CardReference { get; init; }

    public required decimal TransactionAmount { get; init; }

    public required DateOnly TransactionDate { get; init; }

    /// <summary>How much is being claimed. Null means the full amount.</summary>
    public decimal? ClaimedAmount { get; init; }

    /// <summary>When the cardholder reported it. Fraud has a hard deadline.</summary>
    public DateOnly? ReportedOn { get; init; }

    /// <summary>Scheme reason code. Required for disputes, meaningless for fraud.</summary>
    public string? ReasonCode { get; init; }

    /// <summary>Whether the merchant has had its chance to respond.</summary>
    public bool MerchantResponseWindowClosed { get; init; }

    public decimal AmountToSettle => ClaimedAmount ?? TransactionAmount;
}
