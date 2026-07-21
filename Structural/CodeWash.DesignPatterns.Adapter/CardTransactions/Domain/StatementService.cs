namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;

/// <summary>A statement: the lines that make it up and what they add up to.</summary>
public sealed record Statement
{
    public required IReadOnlyList<CardTransaction> Lines { get; init; }

    public required Money Total { get; init; }
}

/// <summary>
/// The Client. It builds a statement out of <see cref="CardTransaction"/> values and
/// depends on <see cref="ICardTransactionSource"/> alone. It has no idea whether the
/// transactions came from Tessura, from Halcyon, or from a stub in a test — which is
/// the entire point of the adapter sitting between them.
/// </summary>
public sealed class StatementService(ICardTransactionSource source)
{
    public async Task<Statement> BuildAsync(
        CardReference card,
        DateRange window,
        CancellationToken cancellationToken = default)
    {
        var lines = await source.GetTransactionsAsync(card, window, cancellationToken);

        // A demo card bills in one currency; sum the spend on it.
        var currency = lines.Count > 0 ? lines[0].Amount.Currency : "EUR";
        var total = new Money(lines.Sum(line => line.Amount.Amount), currency);

        return new Statement { Lines = lines, Total = total };
    }
}
