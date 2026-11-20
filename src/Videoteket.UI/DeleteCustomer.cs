class DeleteCustomer
{
    private readonly CustomerRepository _customerRepository;

    public DeleteCustomer(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public void Run()
    {
        Console.WriteLine("Ditt val: Ta bort kund");
        var customers = _customerRepository.GetAll();

        foreach (var customer in customers)
        {
            Console.WriteLine(
                $"Id: {customer.Id}\nNamn: {customer.Name}\nE-post: {customer.Email}\nTelefon: {customer.Telephone}\nAdress: {customer.Address}\nPersonnummber: {customer.PersonNumber}\n\n");
        }

        Console.WriteLine("Vilken kund vill du ta bort? Id:");

        if (int.TryParse(Console.ReadLine(), out int customerId))
        {
            int antalBorttagna = _customerRepository.DeleteById(customerId);

            Console.WriteLine(antalBorttagna == 1
                ? "Kunden togs bort."
                : "Ingen kund hittades med det ID:t.");
        }
        else
        {
            Console.WriteLine("Ange ett giltigt ID.");
        }
    }
}
