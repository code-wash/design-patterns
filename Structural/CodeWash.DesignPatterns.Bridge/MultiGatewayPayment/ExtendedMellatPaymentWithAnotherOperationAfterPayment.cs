namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class ExtendedMellatPaymentWithAnotherOperationAfterPayment(IPaymentImplementation implementation)
    : Payment(implementation)
{
    public override string PaymentOperation()
    {
        return "Mellat Payment With Another Operation After Payment ~>" + Implementation.DoPayment();
    }
}
