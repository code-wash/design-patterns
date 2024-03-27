namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class BMIPayment : IPaymentImplementation
{
    public string DoPayment()
    {
        return "Bank Melli Payment.\n";
    }
    public string DoVerify()
    {
        return "Bank Melli Verified.\n";
    }
}
