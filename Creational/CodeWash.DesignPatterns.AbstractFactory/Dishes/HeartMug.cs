namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Concrete Product B1
class HeartMug : IMug
{
    public void Display()
    {
        Console.WriteLine("Producing Heart Mug");
    }
}
