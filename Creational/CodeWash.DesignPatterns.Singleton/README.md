# Singleton — one process-wide sequence allocator

> A short, honest one. Singleton is the pattern most changed by modern .NET: the
> dependency-injection container now does its job better in almost every app. This
> example shows the classic form, the one place it still earns its keep, and — loudly —
> when to use `AddSingleton` instead.

## The problem

Something must hand out **unique, ever-increasing numbers** for the lifetime of the
process — say a reference number stamped on each request. The only way that works is if
every caller shares **one** allocator. A second instance would start its own count from
zero and hand back numbers the first one had already issued.

So "there is exactly one of these" is not a convenience here; it is the correctness
condition.

## What varies

Nothing — and that is the point. Unlike Strategy or Adapter, a Singleton has no axis of
variation to absorb. The single invariant it protects is its own uniqueness: one
instance, reached the same way from everywhere, initialised exactly once.

## The naive approach and where it breaks

The first thing people write:

```csharp
public sealed class SequenceGenerator
{
    private static SequenceGenerator? _instance;

    public static SequenceGenerator Current =>
        _instance ??= new SequenceGenerator();   // races: two threads can both see null
}
```

Under load, two threads read `_instance` as null at the same moment and each build one —
now there are two allocators and the numbers collide. The classic fix was **double-checked
locking**: a `lock`, a second null check inside it, and a `volatile` field. It works, but
it is four fiddly lines that are easy to get subtly wrong.

## The pattern

Modern .NET collapses that boilerplate into `Lazy<T>`, which *is* thread-safe once-only
initialisation:

```csharp
private static readonly Lazy<SequenceGenerator> Lazy = new(() => new SequenceGenerator());

public static SequenceGenerator Current => Lazy.Value;

public long Next() => Interlocked.Increment(ref _last);
```

The first thread to read `.Value` runs the factory; the rest block until it is done and
then all receive the same instance. `Interlocked.Increment` makes each draw atomic, so
concurrent callers get distinct numbers rather than trampling each other.

## The payoff — and the caveat that matters more

`SequenceGeneratorTests` shows the guarantee: `Current` is always the same instance, the
numbers strictly increase, and 16 threads drawing 5,000 each get 80,000 distinct
values — no duplicates.

But here is the part to read before you copy this class:

> **In a DI application, don't write this. Register it.**
>
> ```csharp
> services.AddSingleton<SequenceGenerator>();
> ```
>
> You get the *same* one-instance-per-process lifetime, and in exchange:
>
> - **The type stays an ordinary, injectable object.** A test can build its own generator
>   instead of sharing one global. Our own tests here have to reset a static field by
>   reflection between runs — that smell is precisely what DI removes.
> - **The container owns the lifetime**, including disposal and construction order,
>   instead of a static field that lives until the process dies.
> - **No global mutable state** threaded invisibly through the codebase via a static
>   property.

Hand-roll the Singleton only where there is genuinely no container to lean on: a library
with no DI, a static entry point, a `[ModuleInitializer]`. That narrow case is the whole
reason this pattern still appears here.

## Not to be confused with

- **A `static` class** — also one-per-process, but it can never implement an interface, be
  passed as a parameter, or be swapped in a test. A Singleton is still an object; a static
  class is not.
- **DI singleton lifetime** — `AddSingleton` gives you Singleton's *lifetime* without its
  *global access point*. Same guarantee, none of the testability cost. Prefer it.
- **Monostate** — many instances that quietly share static fields, so they behave as one.
  A curiosity; it hides the sharing rather than making it explicit.

## Run it

```bash
dotnet run --project ../CodeWash.DesignPatterns.Singleton.Usage
dotnet test ../CodeWash.DesignPatterns.Singleton.Tests
```
