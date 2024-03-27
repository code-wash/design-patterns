namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class ExtendedMellatPaymentWithAnotherOperationAfterPayment : Payment
{
    public ExtendedMellatPaymentWithAnotherOperationAfterPayment(IPaymentImplementation implementation) : base(implementation)
    {
    }

    public override string PaymentOperation()
    {
        return "Mellat Payment With Another Operation After Payment ~>" + base._implementation.DoPayment();
    }
}
