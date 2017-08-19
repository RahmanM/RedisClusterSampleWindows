using System;
using ObjectDumper;
public class Dumper
{
    public static void Print(string input)
    {
        Console.WriteLine(input);
    }

    public static void Print(Customer input)
    {
        var result = ObjectDumper<Customer>.Dump(input);
        Console.WriteLine(result);
    }

    public static Customer GetCustomer()
    {
        return new Customer()
        {
            Id = 1,
            Name = "rahman",
            Address = new Address() { City = "sydney", Line1 = "address 1", Line2 = "address 2" },
        };
    }
}
