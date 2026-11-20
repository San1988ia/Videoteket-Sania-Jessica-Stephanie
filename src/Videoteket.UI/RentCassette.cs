using MySql.Data.MySqlClient;

class RentCassette
{
    private readonly RentalRepository _rentalRepository;

    public RentCassette(RentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public void Run()
    {
        Console.WriteLine("Ditt val: Låna film");
        List<AvailableCassette> availableCassettes;
        try
        {
            availableCassettes = _rentalRepository.GetAvailableCassettes();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Kassetterna kunde inte hämtas på grund av ett databasfel: {ex.Message}");
            return;
        }

        if (availableCassettes.Count == 0)
        {
            Console.WriteLine("Det finns inga tillgängliga kassetter.");
            return;
        }

        Console.WriteLine("Tillgängliga kassetter:");
        foreach (var availableCassette in availableCassettes)
        {
            int pricePerDay = RentalPricing.GetPricePerDay(availableCassette.IsNew);
            Console.WriteLine(
                $"ID: {availableCassette.Id}, Film: {availableCassette.Title}, " +
                $"Streckkod: {availableCassette.Barcode}, Pris: {pricePerDay} kr/dygn");
        }

        Console.Write("Kundens personnummer (ÅÅÅÅMMDDxxxx): ");
        string? personNumber = Console.ReadLine()?.Trim();
        Console.Write("Kassettens ID: ");
        bool validCassette = int.TryParse(Console.ReadLine(), out int cassetteId);
        Console.Write("Antal dygn: ");
        bool validDays = int.TryParse(Console.ReadLine(), out int days);
        Console.Write("Kvittonummer: ");
        bool validReceipt = int.TryParse(Console.ReadLine(), out int receiptNumber);

        if (string.IsNullOrWhiteSpace(personNumber))
        {
            Console.WriteLine("Ange kundens personnummer.");
            return;
        }

        if (!validCassette || !validDays || !validReceipt ||
            cassetteId <= 0 || days <= 0 || receiptNumber <= 0)
        {
            Console.WriteLine("Kassett-ID, antal dygn och kvittonummer måste vara positiva heltal.");
            return;
        }

        try
        {
            DateTime rentalDate = DateTime.Now;
            DateTime dueDate = rentalDate.Date.AddDays(days);
            int totalPrice = _rentalRepository.RentCassette(
                personNumber, cassetteId, days, receiptNumber, rentalDate, dueDate);

            Console.WriteLine($"Hyran kostar {totalPrice} kr.");
            Console.WriteLine($"Återlämnas senast: {dueDate:yyyy-MM-dd}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Ange ett giltigt antal dygn.");
        }
        catch (OverflowException)
        {
            Console.WriteLine("Antalet dygn är för stort för att beräkna priset.");
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Hyran kunde inte sparas på grund av ett databasfel: {ex.Message}");
        }
    }
}
