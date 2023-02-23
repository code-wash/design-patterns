namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public interface IPaymentImplementation
{
    string DoPayment();
    string DoVerify();
}
