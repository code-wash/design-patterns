using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement.Strategies;

/// <summary>
/// One way of turning a <see cref="SettlementRequest"/> into a
/// <see cref="SettlementDecision"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each strategy declares the claim type it handles rather than the selector
/// keeping a switch statement. With a switch, adding a claim type means editing
/// the selector, and forgetting to is a runtime error. Here it means adding a
/// class -- the selector never changes.
/// </para>
/// <para>
/// It also removes the need for each strategy to defensively re-check the claim
/// type on entry. The selector already guarantees it.
/// </para>
/// </remarks>
public interface ISettlementStrategy
{
    /// <summary>The claim type this strategy is responsible for.</summary>
    ClaimType Handles { get; }

    Task<SettlementDecision> SettleAsync(
        SettlementRequest request,
        CancellationToken cancellationToken = default);
}
