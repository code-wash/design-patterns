namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// Fraud only: the bank carries the loss itself and recovers from the scheme
/// later. There is no merchant to bill.
/// </summary>
public interface ILossProvisionLedger
{
    Task ProvisionAsync(string claimId, decimal amount, CancellationToken cancellationToken = default);
}
