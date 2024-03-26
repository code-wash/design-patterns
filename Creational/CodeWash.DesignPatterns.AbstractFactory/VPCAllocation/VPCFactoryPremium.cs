namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

public class VPCFactoryPremium : IVPCFactory
{
    public string Title => "Premium";

    public ICPU GetCPU() => new CPU2();

    public IRAM GetRAM() => new RAM32();
}
