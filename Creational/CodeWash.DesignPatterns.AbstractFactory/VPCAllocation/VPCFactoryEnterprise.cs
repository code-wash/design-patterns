namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

public class VPCFactoryEnterprise : IVPCFactory
{
    public string Title => "Enterprise";

    public ICPU GetCPU() => new CPU3();

    public IRAM GetRAM() => new RAM32();
}
