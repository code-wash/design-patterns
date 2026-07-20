using CodeWash.DesignPatterns.Strategy.ClaimSettlement.Contracts;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement.Strategies;

/// <summary>
/// Maps a claim type to the strategy responsible for it.
/// </summary>
public interface ISettlementStrategySelector
{
    ISettlementStrategy SelectFor(ClaimType claimType);
}
