namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Halcyon;

// A second third-party SDK, incompatible with ours in an entirely different way from
// Tessura. It is strongly typed and returns everything in one call — but amounts are
// integer minor units, the date is a Unix timestamp, the merchant is nested, and it
// reports entries that never settled. Two vendors, two shapes, one domain port: the
// adapters are where the shapes get reconciled.

public enum HalcyonEntryStatus
{
    Settled,
    Pending,
    Reversed,
}

public sealed record HalcyonCounterparty(string DisplayName);

public sealed record HalcyonEntry(
    long AmountMinorUnits,
    string CurrencyCode,
    long PostedAtUnixSeconds,
    HalcyonCounterparty Counterparty,
    HalcyonEntryStatus Status);

public interface IHalcyonClient
{
    Task<IReadOnlyList<HalcyonEntry>> FetchEntriesAsync(
        string cardToken,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}

/// <summary>Canned, in-memory Halcyon so the example runs without a real processor.</summary>
public sealed class InMemoryHalcyonClient(IReadOnlyList<HalcyonEntry> entries) : IHalcyonClient
{
    public Task<IReadOnlyList<HalcyonEntry>> FetchEntriesAsync(
        string cardToken,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
        => Task.FromResult(entries);
}
