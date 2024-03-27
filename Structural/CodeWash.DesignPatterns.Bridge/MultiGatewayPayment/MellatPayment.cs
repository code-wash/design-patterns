namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class MellatPayment : IPaymentImplementation
{
    public string DoPayment()
    {
        return "Bank Mellat Payment \n";
    }
    public string DoVerify()
    {
        return "Bank Mellat Verified.\n";
    }
}