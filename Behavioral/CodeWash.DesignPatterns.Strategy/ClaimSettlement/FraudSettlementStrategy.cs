using Microsoft.Extensions.Logging;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// Fraud: the cardholder never authorised the transaction.
/// </summary>
/// <remarks>
/// Four steps, three collaborators, and none of them shared with
/// <see cref="DisputeSettlementStrategy"/>: block the card, assess the
/// exposure, escalate if a human is needed, provision the loss. The bank
/// carries this itself and recovers from the scheme later -- there is no
/// merchant to bill.
/// </remarks>
public sealed class FraudSettlementStrategy(
    ICardSecurityService cardSecurity,
    IFraudExposureAssessor exposureAssessor,
    ILossProvisionLedger lossLedger,
    ILogger<FraudSettlementStrategy> logger) : ISettlementStrategy
{
    /// <summary>Days after the transaction within which fraud must be reported.</summary>
    private const int ReportingWindowDays = 60;

    public ClaimType Handles => ClaimType.Fraud;

    public async Task<SettlementDecision> SettleAsync(
        SettlementRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ReportedOn is null)
        {
            return SettlementDecision.Rejected(request.ClaimId, "Fraud claims require a report date.");
        }

        if (request.ClaimedAmount is not null && request.ClaimedAmount != request.TransactionAmount)
        {
            return SettlementDecision.Rejected(
                request.ClaimId,
                "Fraud claims are always settled for the full amount.");
        }

        var daysToReport = request.ReportedOn.Value.DayNumber - request.TransactionDate.DayNumber;
        if (daysToReport > ReportingWindowDays)
        {
            return SettlementDecision.Rejected(
                request.ClaimId,
                $"Reported {daysToReport} days after the transaction, outside the {ReportingWindowDays} day window.");
        }

        // Containment comes first, and only fraud does it.
        await cardSecurity.BlockCardAsync(request.CardReference, cancellationToken).ConfigureAwait(false);

        var exposure = await exposureAssessor.AssessAsync(request, cancellationToken).ConfigureAwait(false);

        if (exposure.RequiresManualReview)
        {
            logger.LogInformation("Claim {ClaimId} escalated to manual fraud review.", request.ClaimId);

            return SettlementDecision.Deferred(
                request.ClaimId,
                "Card blocked; exposure assessment requires manual review before settlement.");
        }

        await lossLedger
            .ProvisionAsync(request.ClaimId, exposure.RecoverableAmount, cancellationToken)
            .ConfigureAwait(false);

        return SettlementDecision.Settled(
            request.ClaimId,
            exposure.RecoverableAmount,
            "issuer-loss-provision",
            "Card blocked and loss provisioned; recovery pursued through the scheme.");
    }
}
