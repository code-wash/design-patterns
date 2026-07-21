using System.Globalization;

namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Tessura;

// Everything in this file stands in for a third-party SDK we do not own and cannot
// change. Its shape is the Adaptee: an integer-status envelope on every call, a
// two-step lookup (resolve an account handle, then query it), cursor pagination, and
// dates and amounts as strings. The adapter exists precisely because we are stuck
// with all of this.

/// <summary>Tessura wraps every response in this envelope, HTTP-style.</summary>
public sealed class TessuraResponse<T>
{
    public int StatusCode { get; init; }

    public T? Result { get; init; }
}

/// <summary>Tessura's internal identifiers for a card. You cannot query without them.</summary>
public sealed class TessuraAccountHandle
{
    public string CustomerId { get; init; } = "";

    public string AccountId { get; init; } = "";
}

public sealed class TessuraTransactionPage
{
    public TessuraTransaction[] Transactions { get; init; } = [];

    /// <summary>Null or empty on the final page.</summary>
    public string? NextCursor { get; init; }
}

public sealed class TessuraTransaction
{
    public string PostedDate { get; init; } = "";   // "yyyy-MM-dd"

    public string Amount { get; init; } = "";        // "12.34"

    public string Currency { get; init; } = "";      // "EUR"

    public string MerchantName { get; init; } = "";
}

public interface ITessuraGateway
{
    Task<TessuraResponse<TessuraAccountHandle>> ResolveAccountAsync(
        string cardNumber, CancellationToken cancellationToken = default);

    Task<TessuraResponse<TessuraTransactionPage>> GetTransactionsAsync(
        string customerId,
        string accountId,
        string fromDate,
        string toDate,
        string cursor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A canned, in-memory Tessura so the example runs without a real processor. It only
/// serves data — it deliberately keeps the awkward envelope-and-cursor shape, because
/// that shape is what the adapter has to absorb.
/// </summary>
public sealed class InMemoryTessuraGateway(
    TessuraAccountHandle handle,
    IReadOnlyList<TessuraTransaction[]> pages) : ITessuraGateway
{
    public Task<TessuraResponse<TessuraAccountHandle>> ResolveAccountAsync(
        string cardNumber, CancellationToken cancellationToken = default)
        => Task.FromResult(new TessuraResponse<TessuraAccountHandle> { StatusCode = 200, Result = handle });

    public Task<TessuraResponse<TessuraTransactionPage>> GetTransactionsAsync(
        string customerId,
        string accountId,
        string fromDate,
        string toDate,
        string cursor,
        CancellationToken cancellationToken = default)
    {
        var index = string.IsNullOrEmpty(cursor) ? 0 : int.Parse(cursor, CultureInfo.InvariantCulture);
        var next = index + 1 < pages.Count ? (index + 1).ToString(CultureInfo.InvariantCulture) : null;

        var page = new TessuraTransactionPage { Transactions = pages[index], NextCursor = next };
        return Task.FromResult(new TessuraResponse<TessuraTransactionPage> { StatusCode = 200, Result = page });
    }
}
