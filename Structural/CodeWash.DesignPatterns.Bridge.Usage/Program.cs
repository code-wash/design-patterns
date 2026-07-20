using CodeWash.DesignPatterns.Bridge.MultiGatewayPayment;

// The client only ever talks to the Payment abstraction. Swapping the gateway
// (the implementation) or adding a refinement (the extended abstraction) does
// not change a line of this code -- that is the point of Bridge.
Run("Bank Melli", new Payment(new BMIPayment()));
Run("Mellat", new Payment(new MellatPayment()));
Run("Mellat + post-payment step", new ExtendedMellatPaymentWithAnotherOperationAfterPayment(new MellatPayment()));

static void Run(string scenario, Payment payment)
{
    Console.WriteLine($"--- {scenario} ---");
    Console.Write(payment.PaymentOperation());
    Console.Write(payment.VerifyOperation());
    Console.WriteLine();
}
