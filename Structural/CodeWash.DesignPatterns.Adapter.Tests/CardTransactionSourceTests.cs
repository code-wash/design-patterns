using CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;
using CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Halcyon;
using CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Tessura;

namespace CodeWash.DesignPatterns.Adapter.Tests;

public sealed class CardTransactionSourceTests
{
    private static readonly CardReference Card = new("card-token-xyz");
    private static readonly DateRange March = new(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

    // The two transactions both vendors will be seeded with, expressed in domain terms.
    private static readonly CardTransaction[] Expected =
    [
        new() { Date = new DateOnly(2026, 3, 14), Amount = new Money(12.34m, "EUR"), Merchant = "Blue Bottle Coffee" },
        new() { Date = new DateOnly(2026, 3, 16), Amount = new Money(250.00m, "EUR"), Merchant = "Northwind Traders" },
    ];

    [Fact]
    public async Task BothAdapters_TranslateEquivalentVendorData_IntoIdenticalDomainTransactions()
    {
        var viaTessura = await SeededTessura().GetTransactionsAsync(Card, March);
        var viaHalcyon = await SeededHalcyon().GetTransactionsAsync(Card, March);

        // Record equality: two very different vendor payloads collapse to the same
        // domain values. That equality is the whole promise of the adapter.
        Assert.Equal(Expected, viaTessura);
        Assert.Equal(Expected, viaHalcyon);
        Assert.Equal(viaTessura, viaHalcyon);
    }

    [Fact]
    public async Task StatementService_ProducesTheSameStatement_WhicheverVendorIsBehindThePort()
    {
        var fromTessura = await new StatementService(SeededTessura()).BuildAsync(Card, March);
        var fromHalcyon = await new StatementService(SeededHalcyon()).BuildAsync(Card, March);

        Assert.Equal(new Money(262.34m, "EUR"), fromTessura.Total);
        Assert.Equal(fromTessura.Total, fromHalcyon.Total);
        Assert.Equal(fromTessura.Lines, fromHalcyon.Lines);
    }

    [Fact]
    public async Task TessuraAdapter_FollowsTheCursor_AndFlattensEveryPageIntoOneList()
    {
        // The seed spreads the two transactions across two pages; the caller sees one list.
        var result = await SeededTessura().GetTransactionsAsync(Card, March);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task TessuraAdapter_TranslatesAVendorFailure_IntoADomainException()
    {
        var source = new TessuraCardSource(new BrokenTessuraGateway());

        // The caller catches a domain exception; Tessura's 503 never escapes the adapter.
        await Assert.ThrowsAsync<CardTransactionSourceException>(
            () => source.GetTransactionsAsync(Card, March));
    }

    [Fact]
    public async Task HalcyonAdapter_ConvertsMinorUnitsEpochAndNestedMerchant()
    {
        var client = new InMemoryHalcyonClient(
        [
            new HalcyonEntry(1234, "EUR", ToEpoch(new DateOnly(2026, 3, 14)),
                new HalcyonCounterparty("Blue Bottle Coffee"), HalcyonEntryStatus.Settled),
        ]);

        var result = await new HalcyonCardSource(client).GetTransactionsAsync(Card, March);

        var line = Assert.Single(result);
        Assert.Equal(new DateOnly(2026, 3, 14), line.Date);
        Assert.Equal(new Money(12.34m, "EUR"), line.Amount);
        Assert.Equal("Blue Bottle Coffee", line.Merchant);
    }

    [Fact]
    public async Task HalcyonAdapter_OmitsEntriesThatNeverSettled()
    {
        var client = new InMemoryHalcyonClient(
        [
            new HalcyonEntry(1234, "EUR", ToEpoch(new DateOnly(2026, 3, 14)),
                new HalcyonCounterparty("Blue Bottle Coffee"), HalcyonEntryStatus.Settled),
            new HalcyonEntry(5000, "EUR", ToEpoch(new DateOnly(2026, 3, 15)),
                new HalcyonCounterparty("Pending Store"), HalcyonEntryStatus.Pending),
            new HalcyonEntry(9900, "EUR", ToEpoch(new DateOnly(2026, 3, 16)),
                new HalcyonCounterparty("Refunded Gadget Co"), HalcyonEntryStatus.Reversed),
        ]);

        var result = await new HalcyonCardSource(client).GetTransactionsAsync(Card, March);

        var line = Assert.Single(result);
        Assert.Equal("Blue Bottle Coffee", line.Merchant);
    }

    private static TessuraCardSource SeededTessura() => new(new InMemoryTessuraGateway(
        new TessuraAccountHandle { CustomerId = "C-100", AccountId = "A-200" },
        [
            [new TessuraTransaction { PostedDate = "2026-03-14", Amount = "12.34", Currency = "EUR", MerchantName = "Blue Bottle Coffee" }],
            [new TessuraTransaction { PostedDate = "2026-03-16", Amount = "250.00", Currency = "EUR", MerchantName = "Northwind Traders" }],
        ]));

    private static HalcyonCardSource SeededHalcyon() => new(new InMemoryHalcyonClient(
    [
        new HalcyonEntry(1234, "EUR", ToEpoch(new DateOnly(2026, 3, 14)),
            new HalcyonCounterparty("Blue Bottle Coffee"), HalcyonEntryStatus.Settled),
        new HalcyonEntry(25000, "EUR", ToEpoch(new DateOnly(2026, 3, 16)),
            new HalcyonCounterparty("Northwind Traders"), HalcyonEntryStatus.Settled),
    ]));

    private static long ToEpoch(DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds();

    /// <summary>A Tessura that fails the way the real one does: a non-200 in the envelope.</summary>
    private sealed class BrokenTessuraGateway : ITessuraGateway
    {
        public Task<TessuraResponse<TessuraAccountHandle>> ResolveAccountAsync(
            string cardNumber, CancellationToken cancellationToken = default)
            => Task.FromResult(new TessuraResponse<TessuraAccountHandle> { StatusCode = 503, Result = null });

        public Task<TessuraResponse<TessuraTransactionPage>> GetTransactionsAsync(
            string customerId, string accountId, string fromDate, string toDate, string cursor,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Should never be reached: the account resolve failed first.");
    }
}
