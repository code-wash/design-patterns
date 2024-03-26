namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

public class VPCFactoryBasic : IVPCFactory
{
    public string Title => "Basic";

    public ICPU GetCPU() => new CPU1();

    public IRAM GetRAM() => new RAM16();
}
