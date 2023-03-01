namespace CodeWash.DesignPatterns.Facade;
public class FacadePattern
{
    public static void Main()
    {
        // Facade
        Mortgage mortgage = new Mortgage();
        // Evaluate mortgage eligibility for customer
        Customer customer = new Customer("Farshad Fahimi");
        bool eligible = mortgage.IsEligible(customer, 1200);
        Console.WriteLine("\n" + customer.Name +
                " has been " + (eligible ? "Approved" : "Rejected"));
        // Wait for user
        Console.ReadKey();
    }
}