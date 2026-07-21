using System.Collections.Concurrent;
using System.Reflection;
using CodeWash.DesignPatterns.Singleton;

namespace CodeWash.DesignPatterns.Singleton.Tests;

public sealed class SequenceGeneratorTests : IDisposable
{
    // The singleton's counter is process-global static state, so it leaks from one test
    // into the next. We reset it around each test to keep them isolated. Needing to do
    // this at all is the hand-rolled Singleton's cost — DI's AddSingleton would let a
    // test build its own generator instead.
    public SequenceGeneratorTests() => ResetCounter();

    public void Dispose() => ResetCounter();

    [Fact]
    public void Current_AlwaysReturnsTheSameInstance()
    {
        Assert.Same(SequenceGenerator.Current, SequenceGenerator.Current);
    }

    [Fact]
    public void Next_ReturnsStrictlyIncreasingValues()
    {
        var a = SequenceGenerator.Current.Next();
        var b = SequenceGenerator.Current.Next();
        var c = SequenceGenerator.Current.Next();

        Assert.Equal(1, a);
        Assert.True(b > a && c > b);
    }

    [Fact]
    public void Next_UnderConcurrentUse_NeverHandsOutADuplicate()
    {
        const int threads = 16;
        const int perThread = 5_000;
        using var open = new ManualResetEventSlim(false);
        var handedOut = new ConcurrentBag<long>();

        var workers = Enumerable.Range(0, threads).Select(_ => new Thread(() =>
        {
            open.Wait();
            for (var i = 0; i < perThread; i++)
            {
                handedOut.Add(SequenceGenerator.Current.Next());
            }
        })).ToList();

        foreach (var worker in workers) worker.Start();
        open.Set();
        foreach (var worker in workers) worker.Join();

        // Every single draw is unique: the one shared instance plus an atomic increment
        // means no two callers ever received the same number.
        Assert.Equal(threads * perThread, handedOut.Count);
        Assert.Equal(threads * perThread, handedOut.Distinct().Count());
    }

    private static void ResetCounter() =>
        typeof(SequenceGenerator)
            .GetField("_last", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(SequenceGenerator.Current, 0L);
}
