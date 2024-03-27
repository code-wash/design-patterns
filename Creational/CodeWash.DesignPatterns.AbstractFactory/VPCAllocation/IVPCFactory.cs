namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

public interface IVPCFactory
{
    string Title { get; }

    ICPU GetCPU();
    IRAM GetRAM();
}
