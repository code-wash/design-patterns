using Microsoft.Extensions.Logging;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// Dispute: the transaction was authorised, but the cardholder is unhappy with
/// what they received.
/// </summary>
/// <remarks>
/// Compare the shape with <see cref="FraudSettlementStrategy"/>. One
/// collaborator instead of three, and a different question entirely: not "how
/// much do we stand to lose" but "may we bill the merchant yet". The card is
/// never blocked -- it was used legitimately.
/// </remarks>
public sealed class DisputeSettlementStrategy(
    IMerchantRecoveryClient merchantRecovery,
    ILogger<DisputeSettlementStrategy> logger) : ISettlementStrategy
{
    /// <summary>Days from the transaction within which a dispute may be raised.</summary>
    private const int ChargebackWindowDays = 120;

    public ClaimType Handles => ClaimType.Dispute;

    public async Task<SettlementDecision> SettleAsync(
        SettlementRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return SettlementDecision.Rejected(request.ClaimId, "Disputes require a scheme reason code.");
        }

        if (request.ClaimedAmount is { } claimed &&
            (claimed <= 0 || claimed > request.TransactionAmount))
        {
            return SettlementDecision.Rejected(
                request.ClaimId,
                "Claimed amount must be positive and no greater than the transaction amount.");
        }

        var ageDays = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - request.TransactionDate.DayNumber;
        if (ageDays > ChargebackWindowDays)
        {
            return SettlementDecision.Rejected(
                request.ClaimId,
                $"Transaction is {ageDays} days old, outside the {ChargebackWindowDays} day chargeback window.");
        }

        // The merchant's right of reply has no equivalent in the fraud path.
        if (!request.MerchantResponseWindowClosed)
        {
            logger.LogInformation("Claim {ClaimId} deferred: merchant may still respond.", request.ClaimId);

            return SettlementDecision.Deferred(
                request.ClaimId,
                "Awaiting merchant response; nothing settles until the window closes.");
        }

        var amount = request.AmountToSettle;

        await merchantRecovery
            .RaiseChargebackAsync(request.ClaimId, amount, request.ReasonCode, cancellationToken)
            .ConfigureAwait(false);

        return SettlementDecision.Settled(
            request.ClaimId,
            amount,
            "merchant-chargeback",
            $"Chargeback raised against the merchant under reason code {request.ReasonCode}.");
    }
}
