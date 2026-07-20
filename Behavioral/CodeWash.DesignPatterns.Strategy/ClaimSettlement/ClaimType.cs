namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// Why the cardholder is claiming money back. This is the value that selects
/// the strategy.
/// </summary>
public enum ClaimType
{
    /// <summary>
    /// The cardholder never authorised the transaction (stolen card, cloned
    /// card, account takeover).
    /// </summary>
    Fraud,

    /// <summary>
    /// The cardholder authorised the transaction but disputes what they got
    /// (goods never arrived, wrong item, cancelled service).
    /// </summary>
    Dispute,
}
