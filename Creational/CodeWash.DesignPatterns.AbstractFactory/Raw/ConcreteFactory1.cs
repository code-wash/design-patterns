namespace CodeWash.DesignPatterns.AbstractFactory.Raw;

// Concrete Factories produce a family of products that belong to a single
// variant. The factory guarantees that resulting products are compatible.
// Note that signatures of the Concrete Factory's methods return an abstract
// product, while inside the method a concrete product is instantiated.
public class ConcreteFactory1 : IAbstractFactory
{
    public IAbstractProductA CreateProductA() => new ConcreteProductA1();

    public IAbstractProductB CreateProductB() => new ConcreteProductB1();
}
