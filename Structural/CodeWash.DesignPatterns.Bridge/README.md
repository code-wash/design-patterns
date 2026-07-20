# Bridge — routing payments through multiple bank gateways

## The problem

You accept payments through several bank gateways (Bank Melli, Mellat, and more
coming). Each gateway has its own API for *pay* and *verify*.

Separately, the business keeps asking for variations on the payment flow itself: a
plain payment, a payment that triggers a reconciliation step afterwards, later maybe
one that needs a pre-authorisation hold.

## What varies

Two things, and they vary **independently**:

- **Which gateway** processes the transaction (Melli, Mellat, …)
- **What flow** wraps the transaction (plain, post-payment step, pre-auth, …)

That word *independently* is the entire trigger for Bridge. Any business rule about
the flow applies to every gateway, and any new gateway must work with every flow.

## The naive approach and where it breaks

The reflex is inheritance:

```
Payment
├── MellatPayment
├── MelliPayment
├── MellatPaymentWithReconciliation
├── MelliPaymentWithReconciliation
├── MellatPaymentWithPreAuth
└── MelliPaymentWithPreAuth
```

With 2 gateways and 3 flows that is 6 classes. Add a third gateway and it is 9. The
class count is the *product* of the two axes, and worse, the reconciliation logic is
now duplicated once per gateway — so a bug fix has to be applied in N places.

## The pattern

Split the two axes into separate hierarchies and connect them by composition:

- **Abstraction** — `Payment`, holds a reference to an implementation
- **Refined abstraction** — `ExtendedMellatPaymentWithAnotherOperationAfterPayment`
- **Implementor** — `IPaymentImplementation`
- **Concrete implementors** — `BMIPayment`, `MellatPayment`

```csharp
var payment  = new Payment(new MellatPayment());
var extended = new ExtendedMellatPaymentWithAnotherOperationAfterPayment(new BMIPayment());
```

Class count becomes the *sum*: 2 gateways + 3 flows = 5 classes, and adding a gateway
costs exactly one new class that every existing flow can immediately use.

## The payoff

`MultiGatewayPaymentTests` demonstrates this directly — `FakeGateway` is defined
inside the test file and works with both the base abstraction and the refinement,
without a single change to either.

## The cost

Be honest about it: Bridge adds a layer of indirection, and if only *one* of your two
axes ever actually varies, it is over-engineering. Reach for it when you can point at
both axes changing for different reasons and on different schedules.

## Run it

```bash
dotnet run --project ../CodeWash.DesignPatterns.Bridge.Usage
dotnet test ../CodeWash.DesignPatterns.Bridge.Tests
```
