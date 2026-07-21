namespace CodeWash.DesignPatterns.Adapter.CardTransactions.Domain;

/// <summary>
/// A card transaction in our own terms. Every vendor's payload gets reshaped
/// into this before it is allowed past an adapter, so nothing above the adapter
/// layer ever sees a vendor's field names, units or date format.
/// </summary>
public sealed record CardTransaction
{
    public required DateOnly Date { get; init; }

    public required Money Amount { get; init; }

    public required string Merchant { get; init; }
}

/// <summary>An amount with its currency. A value, so two equal amounts are equal.</summary>
public readonly record struct Money(decimal Amount, string Currency);

/// <summary>Opaque handle for the card, as our systems refer to it.</summary>
public sealed record CardReference(string Token);

/// <summary>The window a statement covers, inclusive.</summary>
public sealed record DateRange(DateOnly From, DateOnly To);
