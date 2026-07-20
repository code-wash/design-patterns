# Design Patterns, from real problems

Most design-pattern resources show you `ConcreteProductA1` and a pizza factory. You
finish the article understanding the UML and still not knowing when to reach for the
pattern in your own code.

This repository takes the opposite approach. Every pattern here starts from a problem
that actually shows up in production systems — allocating VM tiers, routing payments
through multiple bank gateways — and arrives at the pattern because the problem
demanded it. The code is secondary. **The reasoning is the point.**

Each pattern answers four questions:

1. **The problem** — what real situation are we in?
2. **What varies** — which axis of change is hurting us?
3. **The naive approach** — what do you write first, and where does it break?
4. **The payoff** — what does the pattern buy, and what does it cost?

## Patterns

| Pattern | Category | Real-world example | Status |
| --- | --- | --- | --- |
| [Abstract Factory](Creational/CodeWash.DesignPatterns.AbstractFactory) | Creational | VPC tier allocation (coherent CPU + RAM families) | ✅ |
| [Bridge](Structural/CodeWash.DesignPatterns.Bridge) | Structural | Multi-gateway payment routing | ✅ |
| Facade | Structural | Mortgage eligibility across bank/loan/credit subsystems | 🚧 placeholder |
| Singleton | Creational | — | 🚧 placeholder |

🚧 = code exists but is still textbook-shaped; awaiting a real example.

## Layout

Every pattern follows the same three-project convention:

```
<Category>/CodeWash.DesignPatterns.<Pattern>/         # the pattern itself, library only
<Category>/CodeWash.DesignPatterns.<Pattern>.Usage/   # runnable console demo
<Category>/CodeWash.DesignPatterns.<Pattern>.Tests/   # tests that prove the payoff
```

Two rules keep this honest:

- **No `Main` in a library project.** Libraries stay libraries; demos live in `.Usage`.
- **No blocking console input.** Every demo runs start to finish unattended, so the
  whole solution stays CI-friendly.

The tests are not there for coverage. They exist to *demonstrate the claim* — a test
that swaps a payment gateway without touching the abstraction proves the pattern's
value far better than a paragraph asserting it.

## Build and run

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build                                          # build everything
dotnet test                                           # run all pattern tests
dotnet run --project Structural/CodeWash.DesignPatterns.Bridge.Usage
```

Shared build settings (target framework, nullable, warnings-as-errors) live in
`Directory.Build.props`, so individual `.csproj` files stay nearly empty.

## Contributing

Not open to contributions yet — the example bar is the whole value of this repo and it
needs to be established first. Once a solid core of patterns is in place, this will
open up.

If you want to suggest a real-world scenario for a pattern, open an issue describing
the *problem* rather than the code. Good scenarios are ones where you felt the pain of
not using the pattern.

## License

MIT
