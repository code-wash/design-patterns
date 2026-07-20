namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// The outcome of <see cref="IFraudExposureAssessor"/>. Has no equivalent on
/// the dispute path.
/// </summary>
public sealed record FraudExposure
{
    public required bool RequiresManualReview { get; init; }

    public required decimal RecoverableAmount { get; init; }
}
