namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// Disputes only: the money is clawed back from the merchant rather than
/// absorbed by the bank. This is the sole collaborator the dispute path needs,
/// against three for fraud.
/// </summary>
public interface IMerchantRecoveryClient
{
    Task RaiseChargebackAsync(
        string claimId,
        decimal amount,
        string reasonCode,
        CancellationToken cancellationToken = default);
}
