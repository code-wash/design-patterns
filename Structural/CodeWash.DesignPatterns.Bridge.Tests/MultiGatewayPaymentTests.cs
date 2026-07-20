using CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

namespace CodeWash.DesignPatterns.Bridge.Tests;

/// <summary>
/// These tests exist to demonstrate the payoff of Bridge, not just to cover the
/// code: the abstraction (Payment) and the implementation (IPaymentImplementation)
/// vary independently, so N gateways x M payment flows costs N + M classes
/// instead of N * M.
/// </summary>
public class MultiGatewayPaymentTests
{
    [Fact]
    public void SwappingTheGateway_DoesNotChangeTheAbstraction()
    {
        var melli = new Payment(new BMIPayment());
        var mellat = new Payment(new MellatPayment());

        // Same abstraction type, same call, different gateway behind it.
        Assert.Contains("Bank Melli", melli.PaymentOperation());
        Assert.Contains("Mellat", mellat.PaymentOperation());
    }

    [Fact]
    public void RefinedAbstraction_ReusesAnyExistingGateway()
    {
        // The refinement was written without knowing about MellatPayment, and
        // MellatPayment was written without knowing about the refinement.
        var extended = new ExtendedMellatPaymentWithAnotherOperationAfterPayment(new MellatPayment());

        Assert.Contains("Another Operation After Payment", extended.PaymentOperation());
        Assert.Contains("Mellat", extended.PaymentOperation());
    }

    [Fact]
    public void RefinedAbstraction_InheritsUnchangedOperations()
    {
        var extended = new ExtendedMellatPaymentWithAnotherOperationAfterPayment(new MellatPayment());
        var plain = new Payment(new MellatPayment());

        // Verify was not overridden, so both abstractions behave identically.
        Assert.Equal(plain.VerifyOperation(), extended.VerifyOperation());
    }

    [Fact]
    public void ANewGateway_WorksWithEveryExistingAbstraction_WithoutTouchingThem()
    {
        // Adding a gateway means adding one class. Nothing else changes.
        var payment = new Payment(new FakeGateway());
        var extended = new ExtendedMellatPaymentWithAnotherOperationAfterPayment(new FakeGateway());

        Assert.Contains("FAKE-PAY", payment.PaymentOperation());
        Assert.Contains("FAKE-PAY", extended.PaymentOperation());
    }

    private sealed class FakeGateway : IPaymentImplementation
    {
        public string DoPayment() => "FAKE-PAY";

        public string DoVerify() => "FAKE-VERIFY";
    }
}
