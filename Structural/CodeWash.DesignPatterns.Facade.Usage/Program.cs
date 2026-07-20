using CodeWash.DesignPatterns.Facade;

// The client asks one question of one object. It never learns that eligibility
// requires coordinating Bank, Loan and Credit in a particular order.
var mortgage = new Mortgage();
var customer = new Customer("Farshad Fahimi");

var eligible = mortgage.IsEligible(customer, 125_000);

Console.WriteLine($"{customer.Name} has been {(eligible ? "Approved" : "Rejected")}");
