using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;
using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Strategies;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

/// <summary>
/// The Context in Strategy terms -- what the API endpoint calls.
/// </summary>
/// <remarks>
/// Note how little is here. All claim-type knowledge lives in the strategies,
/// so this class never grows when a claim type is added. Without Strategy, this
/// is the class that accumulates an if/else chain and becomes the file nobody
/// wants to open.
/// </remarks>
public sealed class ClaimSettlementService(ISettlementStrategySelector selector)
{
    public Task<SettlementDecision> SettleAsync(
        SettlementRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var strategy = selector.SelectFor(request.ClaimType);

        return strategy.SettleAsync(request, cancellationToken);
    }
}
