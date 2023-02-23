namespace CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

public static class MultiGatewayPaymentBridge
{
    public static void Main()
    {
        Client client = new Client();

        Payment abstraction;

        Console.WriteLine("1. BMIPayment.");
        Console.WriteLine("2. MellatPayment.");
        Console.WriteLine("3. ExtendedMellatPayment.");
        Console.WriteLine("Enter choice: ");
        int result = Convert.ToInt32(Console.ReadLine());
        switch (result.ToString())
        {
            case "1":
                abstraction = new Payment(new BMIPayment());
                client.ClientCode(abstraction);
                break;
            case "2":
                abstraction = new Payment(new MellatPayment());
                client.ClientCode(abstraction);
                break;
            case "3":
                abstraction = new ExtendedMellatPaymentWithAnotherOperationAfterPayment(new MellatPayment());
                client.ClientCode(abstraction);
                break;
            case "4":
                Console.WriteLine("Invalid Request");
                break;
        }

        Console.ReadKey();
    }
}
