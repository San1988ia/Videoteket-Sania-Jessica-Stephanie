class AddCustomer
{
    private readonly CustomerRepository _customerRepository;

    public AddCustomer(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public void Run()
    {
        Console.WriteLine("Ditt val: Lägg till kund");
        Console.WriteLine("Lägg till namn (Förnamn Efternamn):");
        string name = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Lägg till email (namn@email.com):");
        string email = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Lägg till telefon (1234567890):");
        string telephone = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Lägg till adress (Gatnamn 1, Staden):");
        string address = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Lägg till personnummer (ÅÅÅÅMMDDxxxx):");
        string personNumber = Console.ReadLine() ?? string.Empty;

        var customer = new Customer
        {
            Name = name,
            Email = email,
            Telephone = telephone,
            Address = address,
            PersonNumber = personNumber
        };

        int rowsAdded = _customerRepository.Add(customer);
        Console.WriteLine(rowsAdded > 0
            ? "Kunden är tillagd."
            : "Kunden kunde inte läggas till.");
    }
}
