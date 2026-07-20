using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement.Downstream;

/// <summary>
/// Fraud only: decides how much of the loss is recoverable and whether a human
/// must look at it.
/// </summary>
/// <remarks>
/// This is the genuinely complicated part in production -- many rules, several
/// downstream calls -- and none of it is relevant to the pattern. An interface
/// is exactly the right place to stop: you can see *that* fraud needs an
/// exposure assessment and a dispute does not, without knowing how one is
/// calculated.
/// </remarks>
public interface IFraudExposureAssessor
{
    Task<FraudExposure> AssessAsync(SettlementRequest request, CancellationToken cancellationToken = default);
}
