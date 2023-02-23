namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public class Client
{
    public void ClientCode(Payment abstraction)
    {
        Console.WriteLine(abstraction.PaymentOperation());
        Console.WriteLine(abstraction.VerifyOperation());
    }
}