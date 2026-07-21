using CodeWash.DesignPatterns.Singleton;

// Whoever asks, wherever they ask from, there is one generator.
Console.WriteLine($"Same instance every time: {ReferenceEquals(SequenceGenerator.Current, SequenceGenerator.Current)}");

// Three unrelated call sites, one continuous sequence — because they share the instance.
Console.WriteLine($"first  -> {SequenceGenerator.Current.Next()}");
Console.WriteLine($"second -> {SequenceGenerator.Current.Next()}");
Console.WriteLine($"third  -> {SequenceGenerator.Current.Next()}");

// Hammer it from many threads at once: every number handed out is still unique.
var handedOut = new System.Collections.Concurrent.ConcurrentBag<long>();
Parallel.For(0, 10_000, _ => handedOut.Add(SequenceGenerator.Current.Next()));

Console.WriteLine($"10,000 concurrent draws, distinct values: {handedOut.Distinct().Count()}");
