using CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;

namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Halcyon;

/// <summary>
/// The Adapter for Halcyon. Same domain port as Tessura's, wildly different work
/// behind it: one call, then convert minor units to a decimal amount, a Unix
/// timestamp to a <see cref="DateOnly"/>, and the nested counterparty to a flat
/// merchant name — and drop everything that did not actually settle, because a
/// statement only shows money that moved.
/// </summary>
public sealed class HalcyonCardSource(IHalcyonClient client) : ICardTransactionSource
{
    public async Task<IReadOnlyList<CardTransaction>> GetTransactionsAsync(
        CardReference card,
        DateRange window,
        CancellationToken cancellationToken = default)
    {
        var entries = await client.FetchEntriesAsync(card.Token, window.From, window.To, cancellationToken);

        return entries
            .Where(entry => entry.Status == HalcyonEntryStatus.Settled)
            .Select(Translate)
            .ToList();
    }

    private static CardTransaction Translate(HalcyonEntry entry) => new()
    {
        Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(entry.PostedAtUnixSeconds).UtcDateTime),

        // Minor units to major, assuming a two-decimal currency — enough for the demo.
        Amount = new Money(entry.AmountMinorUnits / 100m, entry.CurrencyCode),

        Merchant = entry.Counterparty.DisplayName,
    };
}
