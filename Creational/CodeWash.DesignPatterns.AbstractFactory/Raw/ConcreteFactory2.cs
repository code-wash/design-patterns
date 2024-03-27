namespace CodeWash.DesignPatterns.AbstractFactory.Raw;

// Each Concrete Factory has a corresponding product variant.
public class ConcreteFactory2 : IAbstractFactory
{
    public IAbstractProductA CreateProductA() => new ConcreteProductA2();

    public IAbstractProductB CreateProductB() => new ConcreteProductB2();
}
