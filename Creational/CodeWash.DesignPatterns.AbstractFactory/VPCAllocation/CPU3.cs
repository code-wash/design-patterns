namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

class CPU3 : ICPU
{
    public double GetClockSpeed() => 3.2;

    public int GetCoreCount() => 8;
}
