using Microsoft.Extensions.DependencyInjection;

namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the settlement service and every strategy behind it.
    /// </summary>
    /// <remarks>
    /// Registering the strategies against the <em>same</em> interface is what
    /// lets the selector take <c>IEnumerable&lt;ISettlementStrategy&gt;</c>.
    /// Adding a claim type is one line here plus one new class; nothing else in
    /// the codebase moves.
    /// </remarks>
    public static IServiceCollection AddClaimSettlement(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ISettlementStrategy, FraudSettlementStrategy>();
        services.AddScoped<ISettlementStrategy, DisputeSettlementStrategy>();

        services.AddScoped<ISettlementStrategySelector, SettlementStrategySelector>();
        services.AddScoped<ClaimSettlementService>();

        return services;
    }
}
