namespace CodeWash.DesignPatterns.AbstractFactory.Dishes;

// Abstract Factory
public interface IFactory
{
    IPlate CreatePlate();
    IMug CreateMug();
}
