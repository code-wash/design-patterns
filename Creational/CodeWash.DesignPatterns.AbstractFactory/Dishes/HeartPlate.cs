namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Concrete Product A1
class HeartPlate : IPlate
{
    public void Display()
    {
        Console.WriteLine("Producing Heart Plate");
    }
}
