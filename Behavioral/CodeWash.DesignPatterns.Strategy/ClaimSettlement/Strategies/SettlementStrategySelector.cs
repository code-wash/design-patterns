using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement.Strategies;

/// <summary>
/// Picks the strategy for a claim type.
/// </summary>
/// <remarks>
/// <para>
/// Strategies arrive as <c>IEnumerable&lt;ISettlementStrategy&gt;</c> and are
/// indexed by the claim type each one declares. There is deliberately no switch
/// here: this class does not know that fraud or disputes exist, so a third
/// claim type never touches it.
/// </para>
/// <para>
/// The common alternative -- a switch calling
/// <c>serviceProvider.GetRequiredService&lt;TStrategy&gt;()</c> -- works, but
/// puts the selector back in the business of knowing every claim type, which is
/// the coupling Strategy exists to remove.
/// </para>
/// <para>
/// This is the one class here that keeps an explicit field rather than using
/// the primary constructor parameter directly: the index is built once, and a
/// <c>readonly</c> field says so in a way a captured parameter cannot.
/// </para>
/// </remarks>
public sealed class SettlementStrategySelector(IEnumerable<ISettlementStrategy> strategies)
    : ISettlementStrategySelector
{
    private readonly IReadOnlyDictionary<ClaimType, ISettlementStrategy> _strategies = BuildIndex(strategies);

    public ISettlementStrategy SelectFor(ClaimType claimType) =>
        _strategies.TryGetValue(claimType, out var strategy)
            ? strategy
            : throw new NotSupportedException($"No settlement strategy is registered for claim type {claimType}.");

    private static IReadOnlyDictionary<ClaimType, ISettlementStrategy> BuildIndex(
        IEnumerable<ISettlementStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        var byClaimType = new Dictionary<ClaimType, ISettlementStrategy>();

        foreach (var strategy in strategies)
        {
            // Fail at startup, not when the first ambiguous claim arrives.
            if (!byClaimType.TryAdd(strategy.Handles, strategy))
            {
                throw new InvalidOperationException(
                    $"More than one {nameof(ISettlementStrategy)} is registered for {strategy.Handles}.");
            }
        }

        return byClaimType;
    }
}
