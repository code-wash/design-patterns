namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Concrete Factory 3 - Party Factory
public class PartyFactory : IFactory
{
    public IPlate CreatePlate()
    {
        return new ClassicPlate();
    }

    public IMug CreateMug()
    {
        return new ClassicMug();
    }
}