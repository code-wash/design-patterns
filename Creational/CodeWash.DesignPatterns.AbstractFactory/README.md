# Abstract Factory — allocating VPC tiers

## The problem

You run a virtual private cloud offering with tiers: Basic, Standard, Premium,
Enterprise. Each tier is a *coherent bundle* of hardware — a particular CPU paired
with a particular amount of RAM. Enterprise is 8 cores at 3.2GHz with 32GB; Basic is
4 cores at 2.8GHz with 16GB.

The parts are not independently selectable. "Enterprise CPU with Basic RAM" is not a
product you sell — it is a bug.

## What varies

The **whole family** of components varies together as the tier changes. That is the
signal for Abstract Factory, and it is what distinguishes it from Factory Method,
which produces one product at a time.

## The naive approach and where it breaks

```csharp
var cpu = tier switch { "Basic" => new CPU1(), "Enterprise" => new CPU3(), ... };
var ram = tier switch { "Basic" => new RAM16(), "Enterprise" => new RAM32(), ... };
```

Two independent switches over the same concept. Nothing stops a caller from reading
the tier once for CPU and a stale value for RAM, and when you add a tier you must
remember to update *every* switch — the compiler will not remind you. Add a third
component (disk, GPU) and it is a third switch in a third place.

## The pattern

One factory per tier, each producing the entire family:

- **Abstract factory** — `IVPCFactory`
- **Concrete factories** — `VPCFactoryBasic`, `VPCFactoryStandard`, `VPCFactoryPremium`, `VPCFactoryEnterprise`
- **Abstract products** — `ICPU`, `IRAM`
- **Concrete products** — `CPU1`/`CPU2`/`CPU3`, `RAM16`/`RAM32`

```csharp
void Provision(IVPCFactory factory)
{
    var cpu = factory.GetCPU();
    var ram = factory.GetRAM();   // guaranteed to match the CPU
}
```

The tier is chosen once, at the point where the factory is selected. Everything
downstream is coherent by construction.

## The payoff

`VPCAllocationTests` shows client code that compiles without naming a single concrete
CPU, RAM or factory type, and asserts a cross-tier invariant (higher tiers are never
weaker) that only holds *because* families cannot be mixed.

Adding a component type means adding one method to `IVPCFactory` — and now the
compiler forces every tier to account for it, which is precisely what the switch
version could not do.

## The cost

Adding a new *product type* touches every factory. Abstract Factory is cheap when
tiers change often and component types rarely, and expensive in the reverse case.

## Run it

```bash
dotnet run --project ../CodeWash.DesignPatterns.AbstractFactory.Usage
dotnet test ../CodeWash.DesignPatterns.AbstractFactory.Tests
```
