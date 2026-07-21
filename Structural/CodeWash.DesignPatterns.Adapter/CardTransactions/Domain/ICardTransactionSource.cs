namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;

/// <summary>
/// The Target: the interface our application already wanted to depend on, written
/// in domain terms and owned by us. Every card processor we integrate with is made
/// to fit <em>this</em> by an adapter — callers never learn which vendor is behind it.
/// </summary>
public interface ICardTransactionSource
{
    Task<IReadOnlyList<CardTransaction>> GetTransactionsAsync(
        CardReference card,
        DateRange window,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Thrown when a source cannot return transactions. Each adapter maps its vendor's
/// own failure shape (an HTTP status, a vendor exception) into this, so a caller
/// can handle "the feed failed" without referencing a single vendor type.
/// </summary>
public sealed class CardTransactionSourceException(string message) : Exception(message);
