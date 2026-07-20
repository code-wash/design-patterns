namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class Payment(IPaymentImplementation implementation)
{
    /// <summary>
    /// The implementation side of the bridge. Exposed to refined abstractions
    /// as a property rather than a protected field: anything visible to
    /// subclasses is API surface, and fields should stay private.
    /// </summary>
    protected IPaymentImplementation Implementation { get; } = implementation;

    public virtual string PaymentOperation()
    {
        return "Payment Operation Called with target bank ~> " +
            Implementation.DoPayment();
    }

    public virtual string VerifyOperation()
    {
        return "Verify Operation Called with target bank ~> " +
            Implementation.DoVerify();
    }
}
