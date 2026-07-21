namespace CodeWash.DesignPatterns.Singleton;

/// <summary>
/// The classic Singleton: exactly one instance for the whole process, created lazily
/// and thread-safely on first use. The job — handing out unique, ever-increasing
/// numbers — is one where a <em>second</em> instance would be a bug: it would start its
/// own count from zero and reissue numbers the first one had already given out.
/// </summary>
/// <remarks>
/// Read this before reaching for it. In a modern .NET application you would almost never
/// write this class. You would register the type once —
/// <c>services.AddSingleton&lt;SequenceGenerator&gt;()</c> — and inject it. The DI
/// container gives you the same one-instance-per-process lifetime, but the type stays an
/// ordinary object: you can hand a fresh one to a test, and you are not carrying global
/// mutable state that every test in the suite shares. (The tests for this class have to
/// reset a static field by reflection between runs — that awkwardness is the cost of the
/// hand-rolled form, and exactly what DI removes.)
///
/// Hand-roll a Singleton only where there is genuinely no container to lean on: library
/// code with no DI, a static entry point, a <c>[ModuleInitializer]</c>. This is that
/// classic form, kept deliberately small.
/// </remarks>
public sealed class SequenceGenerator
{
    // Lazy<T> is the modern one-liner for thread-safe, once-only initialisation. It
    // replaces the old private-static-field-plus-double-checked-lock boilerplate: the
    // first thread to read .Value runs the factory, every other thread blocks until it
    // finishes, and all of them get the same instance.
    private static readonly Lazy<SequenceGenerator> Lazy = new(() => new SequenceGenerator());

    private long _last;

    private SequenceGenerator()
    {
    }

    /// <summary>The one instance. First access builds it; every access after returns it.</summary>
    public static SequenceGenerator Current => Lazy.Value;

    /// <summary>
    /// Hands out the next number. <see cref="Interlocked"/> makes the increment atomic, so
    /// two threads calling at once still get two distinct values rather than colliding.
    /// </summary>
    public long Next() => Interlocked.Increment(ref _last);
}
