namespace CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

class CPU1 : ICPU
{
    public double GetClockSpeed() => 2.8;

    public int GetCoreCount() => 4;
}
