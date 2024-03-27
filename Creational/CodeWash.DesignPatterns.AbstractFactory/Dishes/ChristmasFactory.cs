namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Concrete Factory 2 - Christmas Factory
public class ChristmasFactory : IFactory
{
    public IPlate CreatePlate()
    {
        return new SnowPlate();
    }

    public IMug CreateMug()
    {
        return new SnowMug();
    }
}
