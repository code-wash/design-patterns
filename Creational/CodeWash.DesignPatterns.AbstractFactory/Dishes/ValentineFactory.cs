namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Concrete Factory 1 - Valentine Factory
public class ValentineFactory : IFactory
{
    public IPlate CreatePlate()
    {
        return new HeartPlate();
    }

    public IMug CreateMug()
    {
        return new HeartMug();
    }
}
