namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

public class VPCFactoryStandard : IVPCFactory
{
    public string Title => "Standard";

    public ICPU GetCPU() => new CPU1();

    public IRAM GetRAM() => new RAM32();
}
