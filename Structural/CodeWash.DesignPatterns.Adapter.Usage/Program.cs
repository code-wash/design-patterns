using CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;
using CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Halcyon;
using CodeWash.DesignPatterns.Adapter.CardTransactions.Vendors.Tessura;

// Two processors, holding the *same* two transactions in their own native shapes.
// Tessura: string dates and amounts, spread across two paginated pages.
var tessura = new InMemoryTessuraGateway(
    new TessuraAccountHandle { CustomerId = "C-100", AccountId = "A-200" },
    [
        [new TessuraTransaction { PostedDate = "2026-03-14", Amount = "12.34", Currency = "EUR", MerchantName = "Blue Bottle Coffee" }],
        [new TessuraTransaction { PostedDate = "2026-03-16", Amount = "250.00", Currency = "EUR", MerchantName = "Northwind Traders" }],
    ]);

// Halcyon: minor units, Unix timestamps, nested merchant, plus a reversed entry
// that never settled and must not appear on the statement.
var halcyon = new InMemoryHalcyonClient(
[
    new HalcyonEntry(1234, "EUR", ToEpoch(new DateOnly(2026, 3, 14)), new HalcyonCounterparty("Blue Bottle Coffee"), HalcyonEntryStatus.Settled),
    new HalcyonEntry(25000, "EUR", ToEpoch(new DateOnly(2026, 3, 16)), new HalcyonCounterparty("Northwind Traders"), HalcyonEntryStatus.Settled),
    new HalcyonEntry(9900, "EUR", ToEpoch(new DateOnly(2026, 3, 17)), new HalcyonCounterparty("Refunded Gadget Co"), HalcyonEntryStatus.Reversed),
]);

var card = new CardReference("card-token-xyz");
var march = new DateRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

// The client is built once against the port, then handed each adapter in turn. Its
// code does not change and does not know which processor it is talking to.
await PrintStatement("Tessura", new StatementService(new TessuraCardSource(tessura)));
await PrintStatement("Halcyon", new StatementService(new HalcyonCardSource(halcyon)));

Console.WriteLine("Same statement, two unrelated vendor APIs — the adapters absorbed every difference.");

async Task PrintStatement(string vendor, StatementService service)
{
    var statement = await service.BuildAsync(card, march);

    Console.WriteLine($"Statement via {vendor}:");
    foreach (var line in statement.Lines)
    {
        Console.WriteLine($"  {line.Date:yyyy-MM-dd}  {line.Amount.Amount,8:0.00} {line.Amount.Currency}  {line.Merchant}");
    }

    Console.WriteLine($"  {"Total",12} {statement.Total.Amount,8:0.00} {statement.Total.Currency}");
    Console.WriteLine();
}

static long ToEpoch(DateOnly date) =>
    new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeSeconds();
