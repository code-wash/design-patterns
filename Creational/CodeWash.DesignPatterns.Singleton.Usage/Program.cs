using CodeWash.DesignPatterns.Singleton.ThreadSafeSingleton;

// Both calls return the same instance; the second argument is ignored because
// initialisation only happens once.
var first = Singleton.GetInstance("FIRST");
var second = Singleton.GetInstance("SECOND");

Console.WriteLine($"first.Value  = {first.Value}");
Console.WriteLine($"second.Value = {second.Value}");
Console.WriteLine($"Same instance: {ReferenceEquals(first, second)}");
