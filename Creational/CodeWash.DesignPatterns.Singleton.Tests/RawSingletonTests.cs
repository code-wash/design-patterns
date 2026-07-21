using System.Reflection;
using Raw = CodeWash.DesignPatterns.Singleton.RawSingleton.Singleton;

namespace CodeWash.DesignPatterns.Singleton.Tests;

/// <summary>
/// The unthreaded singleton. Its whole promise is: however many times you ask,
/// you get the one instance back.
/// </summary>
public sealed class RawSingletonTests : IDisposable
{
    // A singleton keeps its instance in a static field, so it survives from one
    // test into the next. That is the pattern's global state biting the tests
    // that exercise it -- so we null the field out before and after each test to
    // keep them isolated. Needing this at all is a fair criticism of the pattern.
    public RawSingletonTests() => ResetInstance();

    public void Dispose() => ResetInstance();

    [Fact]
    public void GetInstance_CalledTwice_ReturnsTheSameObject()
    {
        var first = Raw.GetInstance();
        var second = Raw.GetInstance();

        Assert.Same(first, second);
    }

    [Fact]
    public void GetInstance_CalledManyTimes_NeverConstructsASecondObject()
    {
        var instances = Enumerable
            .Range(0, 100)
            .Select(_ => Raw.GetInstance())
            .Distinct()
            .ToList();

        Assert.Single(instances);
    }

    private static void ResetInstance() =>
        typeof(Raw)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, null);
}
