using CodeWash.DesignPatterns.AbstractFactory.VPCAllocation;

namespace CodeWash.DesignPatterns.AbstractFactory.Tests;

/// <summary>
/// These tests exist to demonstrate the payoff of Abstract Factory: a tier is
/// chosen once, and the CPU/RAM pair that comes back is guaranteed to be a
/// coherent family. Client code never picks the parts individually, so an
/// invalid combination (Enterprise CPU with Basic RAM) is unrepresentable.
/// </summary>
public class VPCAllocationTests
{
    public static TheoryData<IVPCFactory, string, int, int, double> Tiers => new()
    {
        // factory, title, cores, ram GB, clock
        { new VPCFactoryBasic(),      "Basic",      4, 16, 2.8 },
        { new VPCFactoryStandard(),   "Standard",   4, 32, 2.8 },
        { new VPCFactoryPremium(),    "Premium",    8, 32, 2.8 },
        { new VPCFactoryEnterprise(), "Enterprise", 8, 32, 3.2 },
    };

    [Theory]
    [MemberData(nameof(Tiers))]
    public void EachTier_ProducesItsMatchingCpuAndRamFamily(
        IVPCFactory factory, string title, int cores, int ramGb, double clock)
    {
        Assert.Equal(title, factory.Title);
        Assert.Equal(cores, factory.GetCPU().GetCoreCount());
        Assert.Equal(clock, factory.GetCPU().GetClockSpeed());
        Assert.Equal(ramGb, factory.GetRAM().GetCapacity());
    }

    [Fact]
    public void ClientCode_NeedsOnlyTheAbstractions()
    {
        // Note the parameter type: this method compiles without referencing a
        // single concrete CPU, RAM or factory class. New tiers plug straight in.
        static string Describe(IVPCFactory factory) =>
            $"{factory.Title}: {factory.GetCPU().GetCoreCount()} cores / {factory.GetRAM().GetCapacity()}GB";

        Assert.Equal("Basic: 4 cores / 16GB", Describe(new VPCFactoryBasic()));
        Assert.Equal("Enterprise: 8 cores / 32GB", Describe(new VPCFactoryEnterprise()));
    }

    [Fact]
    public void HigherTiers_AreNeverWeakerThanLowerTiers()
    {
        // The family guarantee is what makes this invariant hold at all.
        IVPCFactory[] ascending =
        [
            new VPCFactoryBasic(),
            new VPCFactoryStandard(),
            new VPCFactoryPremium(),
            new VPCFactoryEnterprise(),
        ];

        for (var i = 1; i < ascending.Length; i++)
        {
            var previous = ascending[i - 1];
            var current = ascending[i];

            Assert.True(current.GetCPU().GetCoreCount() >= previous.GetCPU().GetCoreCount());
            Assert.True(current.GetRAM().GetCapacity() >= previous.GetRAM().GetCapacity());
        }
    }
}
