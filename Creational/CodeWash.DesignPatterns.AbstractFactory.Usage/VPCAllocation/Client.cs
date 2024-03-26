using CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

namespace CodeWash.DesignPatterns.AbstractFactory.Usage.VPCAllocation
{
    // The client code works with factories and products only through abstract
    // types: AbstractFactory and AbstractProduct. This lets you pass any
    // factory or product subclass to the client code without breaking it.
    public class Client
    {
        public void Main()
        {
            // The client code can work with any concrete factory class.
            Console.WriteLine();

            //EmployeeType

            ClientMethod(new VPCFactoryBasic());

            ClientMethod(new VPCFactoryStandard());

            ClientMethod(new VPCFactoryPremium());

            ClientMethod(new VPCFactoryEnterprise());
        }

        public void ClientMethod(IVPCFactory factory)
        {
            Console.WriteLine($"Factory type: {factory.Title}");
            
            var cpu = factory.GetCPU();
            var ram = factory.GetRAM();

            Console.WriteLine($"RAM Capacity: {ram.GetCapacity()}");
            Console.WriteLine($"CPU Cores: {cpu.GetCoreCount()}, Clock Speed: {cpu.GetClockSpeed()}");

            Console.WriteLine("-------");
        }
    }
}
