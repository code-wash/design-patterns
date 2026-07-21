using System.Collections.Concurrent;
using System.Reflection;
using Safe = CodeWash.DesignPatterns.Singleton.ThreadSafeSingleton.Singleton;

namespace CodeWash.DesignPatterns.Singleton.Tests;

/// <summary>
/// The double-checked-locking singleton. It adds two guarantees over the raw one:
/// concurrent callers still see a single instance, and the value passed to the
/// very first call is the one that sticks.
/// </summary>
public sealed class ThreadSafeSingletonTests : IDisposable
{
    public ThreadSafeSingletonTests() => ResetInstance();

    public void Dispose() => ResetInstance();

    [Fact]
    public void GetInstance_CalledTwice_ReturnsTheSameObject()
    {
        var first = Safe.GetInstance("alpha");
        var second = Safe.GetInstance("beta");

        Assert.Same(first, second);
    }

    [Fact]
    public void GetInstance_KeepsTheValueFromTheFirstCall_AndIgnoresLaterOnes()
    {
        Safe.GetInstance("first-caller-wins");
        var later = Safe.GetInstance("too-late");

        Assert.Equal("first-caller-wins", later.Value);
    }

    [Fact]
    public void GetInstance_UnderConcurrentFirstAccess_HandsOutOneInstanceToEveryone()
    {
        const int callers = 32;
        using var open = new ManualResetEventSlim(false);
        var seen = new ConcurrentBag<Safe>();

        // Dedicated threads, not pool tasks: the pool injects new threads only a
        // couple a second, which would stagger the callers and hide any race. Real
        // threads all park on the gate, then hit GetInstance the instant it opens.
        var threads = Enumerable.Range(0, callers).Select(i => new Thread(() =>
        {
            open.Wait();
            seen.Add(Safe.GetInstance($"caller-{i}"));
        })).ToList();

        foreach (var thread in threads) thread.Start();
        open.Set();
        foreach (var thread in threads) thread.Join();

        Assert.Equal(callers, seen.Count);
        Assert.Single(seen.Distinct());
    }

    private static void ResetInstance() =>
        typeof(Safe)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, null);
}
