namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class Payment
{
    protected IPaymentImplementation _implementation;

    public Payment(IPaymentImplementation implementation)
    {
        this._implementation = implementation;
    }

    public virtual string PaymentOperation()
    {
        return "Payment Operation Called with target bank ~> " +
            _implementation.DoPayment();
    }

    public virtual string VerifyOperation()
    {
        return "Verify Operation Called with target bank ~> " +
            _implementation.DoVerify();
    }
}
