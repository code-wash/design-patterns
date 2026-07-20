# Strategy — settling fraud vs. dispute claims

## The problem

A card issuer exposes one endpoint for "the cardholder wants their money back".
One route, one request model, one response model — but two very different kinds of
claim arrive through it:

- **Fraud** — the cardholder never authorised the transaction (stolen card, cloned
  card, account takeover).
- **Dispute** — the cardholder *did* authorise it but is unhappy with what
  they received (goods never arrived, wrong item, cancelled service).

They look alike on the wire and are handled almost nothing alike.

## What varies

The **algorithm**, not the interface. Both claim types take the same
`SettlementRequest` and produce the same `SettlementDecision`. Between those two
points, almost nothing is shared:

| | Fraud | Dispute |
| --- | --- | --- |
| Card is blocked | Yes, first thing | Never — it was used legitimately |
| Who is billed | The bank's own loss provision | The merchant |
| Merchant may respond first | No | Yes — nothing settles until then |
| Partial amounts | Never | Normal |
| Reason code | Meaningless | Mandatory |
| Reporting window | 60 days from transaction | 120-day chargeback window |
| Downstream systems | 3 | 1 |

That last row is the strongest signal. These are not one algorithm with different
constants — they call **different services in a different order**. `FraudSettlementStrategy`
depends on `ICardSecurityService`, `IFraudExposureAssessor` and `ILossProvisionLedger`;
`DisputeSettlementStrategy` depends on `IMerchantRecoveryClient` and nothing
else. They share no collaborator at all.

## The naive approach and where it breaks

```csharp
public async Task<SettlementDecision> SettleAsync(SettlementRequest request)
{
    if (request.ClaimType == ClaimType.Fraud)
    {
        if (request.ReportedOn is null) return Rejected("...");
        await cardSecurity.BlockCardAsync(...);
        var exposure = await exposureAssessor.AssessAsync(...);
        // ...
    }
    else if (request.ClaimType == ClaimType.Dispute)
    {
        if (request.ReasonCode is null) return Rejected("...");
        if (!request.MerchantResponseWindowClosed) return Deferred("...");
        // ...
    }
}
```

This works, right up until it doesn't:

- The class needs **every** downstream dependency injected, whether the request in
  hand uses it or not. Four collaborators today, more with every claim type.
- Testing the dispute path means constructing fraud collaborators that will never
  be called.
- Unrelated rules share a method body, so a change to disputes can break fraud
  through a shared local or an early return.
- Every new claim type makes the one method longer, and it never gets shorter.

## The pattern

- **Strategy** — `ISettlementStrategy`
- **Concrete strategies** — `FraudSettlementStrategy`, `DisputeSettlementStrategy`
- **Context** — `ClaimSettlementService`, which holds a selector and nothing else

```csharp
var strategy = selector.SelectFor(request.ClaimType);
return strategy.SettleAsync(request, cancellationToken);
```

Each strategy now declares only the dependencies it actually uses, and the context
declares none of them.

### Selecting the strategy: the part textbooks skip

Every tutorial writes `new ConcreteStrategyA()`. Real .NET code resolves through DI,
and *how* matters. The common version is a switch:

```csharp
// Works, but the factory now knows every claim type.
return request.ClaimType switch
{
    ClaimType.Fraud          => provider.GetRequiredService<FraudSettlementStrategy>(),
    ClaimType.Dispute        => provider.GetRequiredService<DisputeSettlementStrategy>(),
    _ => throw new NotImplementedException(),
};
```

That reintroduces the coupling Strategy was meant to remove: adding a claim type
means editing the factory, and forgetting to is a runtime failure. It also tempts
each strategy into defensively re-checking the claim type on entry.

The alternative used here is for each strategy to **declare what it handles**:

```csharp
public ClaimType Handles => ClaimType.Fraud;
```

`SettlementStrategySelector` takes `IEnumerable<ISettlementStrategy>` and indexes
them. It contains no switch and does not know fraud exists. Adding a claim type is
one new class plus one registration line. Duplicate registrations throw at
construction rather than at 3am.

## The payoff

`ClaimSettlementTests` demonstrates it. The sharpest test asserts the *call
sequence*: fraud produces `block-card → assess-exposure → provision-loss`, a dispute
produces `raise-chargeback`, and the two sets do not intersect. Others show that a
missing reason code sinks a dispute while fraud settles fine without one, and that a
90-day-old transaction is rejected as fraud but accepted as a dispute. One test walks
`Enum.GetValues<ClaimType>()`, so forgetting to register a future strategy fails the
build rather than production.

## A note on how much domain is in here

The rules in these strategies are real but heavily thinned. In a production issuer,
`IFraudExposureAssessor` alone hides a stack of rules and several remote calls.

That detail is deliberately behind an interface. The pattern is justified by the
*shape* of the variation, not its volume — and past a certain point extra domain
detail teaches card schemes rather than Strategy. Interfaces are exactly the right
place to draw that line: you can see that fraud needs an exposure assessment and a
dispute does not, without needing to know how one is calculated.

## When Strategy is premature

Worth being honest, because this is the most common way the pattern goes wrong.

Strategy costs a layer of indirection, and pays for itself only when the algorithms
genuinely differ. It is easy to build the whole apparatus — interface, two
implementations, selector, registration — and end up with two classes whose bodies
are identical except for a guard clause, because the divergence you expected has not
arrived yet. Then you have not decoupled anything; you have duplicated a method and
added a lookup, and the two copies drift the first time someone fixes a bug in one.

The check is simple: **write the naive `if/else` first.** If the branches contain
substantially the same code, you do not need Strategy yet. Extract it the day they
actually diverge — that day is obvious, and the refactor is cheap because the
difference is right in front of you.

Scaffolding Strategy ahead of *known, scheduled* divergence is defensible; if two
rule sets are landing shortly, possibly from two people at once, having the seam
ready is worth more than it costs. Scaffolding it ahead of *hypothetical* divergence
usually is not.

## Not to be confused with

- **Template Method** — fixed skeleton, subclasses fill in steps. Use it when the
  *sequence* is shared and only individual steps vary. Here the sequences differ
  outright, which is why Strategy fits and Template Method would not.
- **Chain of Responsibility** — each handler decides whether to take the request and
  may pass it on. Use it when the handler must be *discovered*. In Strategy exactly
  one applies and the selector knows which.
- **State** — structurally near-identical, but the object swaps its own strategy as
  its lifecycle advances. A Strategy is chosen by the caller and stays put.

## Run it

```bash
dotnet run --project ../CodeWash.DesignPatterns.Strategy.Usage
dotnet test ../CodeWash.DesignPatterns.Strategy.Tests
```
