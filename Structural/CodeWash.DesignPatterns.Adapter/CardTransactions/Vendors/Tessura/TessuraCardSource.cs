using System.Globalization;
using CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;

namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Tessura;

/// <summary>
/// The Adapter for Tessura. It implements the domain <see cref="ICardTransactionSource"/>
/// by driving the <see cref="ITessuraGateway"/> underneath and reshaping what comes back.
/// The translation is real, not a rename: it runs the two-step lookup, follows the
/// cursor to the last page, and parses Tessura's string dates and amounts into
/// <see cref="DateOnly"/> and <see cref="decimal"/>.
/// </summary>
public sealed class TessuraCardSource(ITessuraGateway gateway) : ICardTransactionSource
{
    public async Task<IReadOnlyList<CardTransaction>> GetTransactionsAsync(
        CardReference card,
        DateRange window,
        CancellationToken cancellationToken = default)
    {
        // Tessura cannot be queried by our card token: first resolve its own
        // customer/account handle, then query transactions against that.
        var handle = Unwrap(await gateway.ResolveAccountAsync(card.Token, cancellationToken));

        var from = window.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = window.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var transactions = new List<CardTransaction>();
        var cursor = "";
        do
        {
            var page = Unwrap(await gateway.GetTransactionsAsync(
                handle.CustomerId, handle.AccountId, from, to, cursor, cancellationToken));

            transactions.AddRange(page.Transactions.Select(Translate));
            cursor = page.NextCursor ?? "";
        }
        while (cursor.Length > 0);

        return transactions;
    }

    private static CardTransaction Translate(TessuraTransaction transaction) => new()
    {
        Date = DateOnly.ParseExact(transaction.PostedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
        Amount = new Money(decimal.Parse(transaction.Amount, CultureInfo.InvariantCulture), transaction.Currency),
        Merchant = transaction.MerchantName,
    };

    // The envelope check Tessura forces on every call, written once here instead of
    // copy-pasted down each method — and translated into a domain exception so the
    // vendor's status codes never travel any further up.
    private static T Unwrap<T>(TessuraResponse<T> response)
    {
        if (response.StatusCode != 200 || response.Result is null)
        {
            throw new CardTransactionSourceException($"Tessura returned status {response.StatusCode}.");
        }

        return response.Result;
    }
}
