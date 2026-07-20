namespace CodeWash.DesignPatterns.Strategy.ClaimSettlement.Downstream;

/// <summary>
/// Fraud only: stop the bleeding before anything else. A dispute never blocks
/// the card, because the card was used legitimately.
/// </summary>
/// <remarks>
/// The implementation is deliberately out of scope. In a real issuer this hides
/// a stack of rules and remote calls; reproducing that here would teach card
/// schemes rather than Strategy.
/// </remarks>
public interface ICardSecurityService
{
    Task BlockCardAsync(string cardReference, CancellationToken cancellationToken = default);
}
