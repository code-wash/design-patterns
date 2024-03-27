using CodeWash.DesignPatterns.AbstractFactory.Dishes;

namespace CodeWash.DesignPatterns.AbstractFactory.Usage.Dishes
{
    // The client code works with factories and products only through abstract
    // types: AbstractFactory and AbstractProduct. This lets you pass any
    // factory or product subclass to the client code without breaking it.
    public class Client
    {
        public void Main()
        {
            Console.WriteLine("Client: Testing client code with the factory type: Valentine");
            ClientMethod(new ValentineFactory());

            Console.WriteLine();

            Console.WriteLine("Client: Testing client code with the factory type: Christmas");
            ClientMethod(new ChristmasFactory());

            Console.WriteLine();

            Console.WriteLine("Client: Testing client code with the factory type: Party");
            ClientMethod(new PartyFactory());
        }

        void ClientMethod(IFactory factory)
        {
            factory.CreatePlate().Display(); // Producing Heart Plate
            factory.CreateMug().Display(); // Producing Heart Mug
        }
    }
}
