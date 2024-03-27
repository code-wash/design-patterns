namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Concrete Product A3
class ClassicPlate : IPlate
{
    public void Display()
    {
        Console.WriteLine("Producing Classic Plate");
    }
}
