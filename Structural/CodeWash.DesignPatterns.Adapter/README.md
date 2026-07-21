# Adapter — one card-transaction feed, many processors

## The problem

Your application wants to read a card's transactions. You write the interface you
*wish* existed — clean domain types, one method — and build everything on top of it:

```csharp
public interface ICardTransactionSource
{
    Task<IReadOnlyList<CardTransaction>> GetTransactionsAsync(
        CardReference card, DateRange window, CancellationToken cancellationToken = default);
}
```

Then reality arrives in the form of a third-party card processor whose SDK looks
nothing like that — envelopes, status codes, pagination, its own field names and
units — and which you cannot change. Later, a second processor arrives, incompatible
with the first in entirely different ways. Neither one fits `ICardTransactionSource`,
and you are not allowed to make them.

## What varies

The **shape of the thing you're integrating**, not what you want from it. Both
processors in this example answer the same question — "what did this card spend?" —
and both are wrapped to the same port. Everything else about them differs:

| | Tessura | Halcyon |
| --- | --- | --- |
| Response wrapper | `{ statusCode, result }` envelope, throw on non-200 | typed result, its own exceptions |
| Finding the card | two steps: resolve an account handle, then query it | one step: query by card token |
| Paging | cursor — you loop until it runs out | returns everything at once |
| Amounts | decimal *string* + currency string (`"12.34"`, `"EUR"`) | int64 **minor units** + code (`1234`, `"EUR"`) |
| Dates | `"yyyy-MM-dd"` string | Unix epoch seconds |
| Merchant | flat `MerchantName` | nested `Counterparty.DisplayName` |
| Non-settled entries | not returned | returned — must be filtered out |

None of that is allowed to reach the application. The adapter is where it stops.

## The naive approach and where it breaks

Skip the port and let the processor's SDK spread through the codebase:

```csharp
var envelope = await tessura.ResolveAccountAsync(pan);
if (envelope.StatusCode != 200) throw ...;
var handle = envelope.Result;
// ... cursor loop, string parsing, TessuraTransaction everywhere a transaction is used
```

It compiles and ships. Then:

- **The vendor's vocabulary leaks everywhere.** `TessuraTransaction`, its envelope and
  its status codes end up in services, tests and mappers that have no business knowing
  the processor's name.
- **Swapping or adding a processor is a rewrite,** not a new class — every call site
  that touched the SDK has to change.
- **Two processors means two code paths** threaded through the same callers, each
  re-deriving "is this settled", "what's the amount", "which page am I on".
- **Vendor quirks become domain bugs** — minor-units-vs-decimal, epoch-vs-string dates,
  handled inline and inconsistently.

## The pattern

- **Target** — `ICardTransactionSource`, the interface we own and wanted all along.
- **Adaptee** — the vendor SDK we cannot change (`ITessuraGateway`, `IHalcyonClient`
  and their envelopes, cursors and minor units).
- **Adapter** — `TessuraCardSource`, `HalcyonCardSource`: each implements the Target by
  driving one Adaptee and translating between the two shapes.
- **Client** — `StatementService`, which depends on the Target and never learns which
  processor answered.

```csharp
// Same client, either processor — the only thing that changes is which adapter
// you construct (or, in a real app, which one you register in DI).
new StatementService(new TessuraCardSource(tessura));
new StatementService(new HalcyonCardSource(halcyon));
```

### The translation is the point

An adapter that just renames fields is barely earning its place. These do real work:

- `TessuraCardSource` runs the two-step lookup, follows the cursor to the last page,
  unwraps the envelope (once, in a `Unwrap<T>` helper rather than copy-pasted down every
  method), and turns its status codes into a domain `CardTransactionSourceException` so
  a vendor's `503` never travels up the stack.
- `HalcyonCardSource` converts minor units to a decimal amount, a Unix timestamp to a
  `DateOnly`, the nested counterparty to a flat merchant, and drops everything that
  never settled — because a statement only shows money that actually moved.

## The payoff

`CardTransactionSourceTests` proves it. The sharpest test seeds both processors with
the *same two purchases* in their own native shapes and asserts the two adapters
produce **identical** `CardTransaction` lists — record equality collapsing two
unrelated payloads to one domain value. Another builds one `StatementService`, hands it
each adapter in turn, and gets the same statement both times: the client is genuinely
decoupled. The rest pin the individual translations — cursor flattening, minor-units and
epoch conversion, dropping non-settled entries, and turning a vendor `503` into a domain
exception.

## Layout

```
CardTransactions/
├── Domain/           the Target port, the domain models, and the Client (StatementService)
└── Vendors/
    ├── Tessura/      Adaptee (envelope + cursor + strings) and its adapter
    └── Halcyon/      Adaptee (minor units + epoch + nesting) and its adapter
```

Each vendor folder holds the foreign SDK shape, a canned in-memory stand-in so the
example runs without a real processor, and the adapter that reconciles the two. The
`InMemory…` types only serve data — they keep the awkward vendor shape on purpose,
because that shape is exactly what the adapter has to absorb.

## Not to be confused with

- **Bridge** (also in this repo, `../CodeWash.DesignPatterns.Bridge`) — structurally a
  near-twin, opposite intent. You reach for **Bridge up front**, designing *both* an
  abstraction and an implementation hierarchy so they can vary independently — you own
  both sides. You reach for **Adapter after the fact**, to make an interface you were
  *handed* fit one you already have. Bridge is a decision; Adapter is a rescue. If you
  can change both interfaces, you're probably doing Bridge; if one is imposed on you,
  it's Adapter.
- **Facade** — also wraps something, but to *simplify* a subsystem you own, not to make a
  foreign interface conform to an expected one. A Facade invents a convenient interface;
  an Adapter conforms to a required one.
- **Decorator** — same interface in and out, adding behaviour. An Adapter changes the
  interface and adds no behaviour of its own beyond translation.

## Run it

```bash
dotnet run --project ../CodeWash.DesignPatterns.Adapter.Usage
dotnet test ../CodeWash.DesignPatterns.Adapter.Tests
```
