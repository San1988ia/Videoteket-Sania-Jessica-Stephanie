using System.Data;
using System.Security.Cryptography;
using Dapper;
using Org.BouncyCastle.Math.EC;

class MainMenu
{
    private readonly IDbConnection _db;

    public MainMenu(IDbConnection db)
    {
        _db = db;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("Hej! Välkommen till Videoteket.");
            Console.WriteLine("Välj ditt menyval nedan:");
            Console.WriteLine("1: Se utbud");
            Console.WriteLine("2: Låna film");
            Console.WriteLine("3: Återlämna film");
            Console.WriteLine("4: Lägga till kund");
            Console.WriteLine("5: Ta bort kund");
            Console.WriteLine("6: Ändra status på film");
            Console.WriteLine("7: Se uthyrda filmer");
            Console.WriteLine("8: Räkna ut sen återlämningskostnad");
            Console.WriteLine("9: Avsluta");

            switch (Console.ReadLine())
            {
                case "1":
                    var titlar = _db.Query<string>("SELECT Title FROM Film");
                    foreach (var titel in titlar)
                    {
                        Console.WriteLine(titel);
                    }
                    break;
                case "2":
                {
                    Console.WriteLine("Ditt val: Låna film");
                    List<AvailableCassette> availableCassettes;
                    try
                    {
                        availableCassettes = _db.Query<AvailableCassette>(
                            @"SELECT fc.Id, fc.Barcode, f.Title, f.IsNew
                              FROM FilmCassette fc
                              JOIN Film f ON f.Id = fc.FilmId
                              WHERE NOT EXISTS (
                                  SELECT 1
                                  FROM RentFilm rf
                                  WHERE rf.CassetteId = fc.Id
                                    AND rf.ReturnDate IS NULL
                              )
                              ORDER BY f.Title, fc.Id").ToList();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        Console.WriteLine($"Kassetterna kunde inte hämtas på grund av ett databasfel: {ex.Message}");
                        break;
                    }

                    if (availableCassettes.Count == 0)
                    {
                        Console.WriteLine("Det finns inga tillgängliga kassetter.");
                        break;
                    }

                    Console.WriteLine("Tillgängliga kassetter:");
                    foreach (var availableCassette in availableCassettes)
                    {
                        int pricePerDay = availableCassette.IsNew ? 49 : 29;
                        Console.WriteLine(
                            $"ID: {availableCassette.Id}, Film: {availableCassette.Title}, " +
                            $"Streckkod: {availableCassette.Barcode}, Pris: {pricePerDay} kr/dygn");
                    }

                    Console.Write("Kundens personnummer (ÅÅÅÅMMDDxxxx): ");
                    string? rentalPersonNumber = Console.ReadLine()?.Trim();
                    Console.Write("Kassettens ID: ");
                    bool validCassette = int.TryParse(Console.ReadLine(), out int cassetteId);
                    Console.Write("Antal dygn: ");
                    bool validDays = int.TryParse(Console.ReadLine(), out int days);
                    Console.Write("Kvittonummer: ");
                    bool validReceipt = int.TryParse(Console.ReadLine(), out int receiptNumber);

                    if (string.IsNullOrWhiteSpace(rentalPersonNumber))
                    {
                        Console.WriteLine("Ange kundens personnummer.");
                        break;
                    }

                    if (!validCassette || !validDays || !validReceipt ||
                        cassetteId <= 0 || days <= 0 || receiptNumber <= 0)
                    {
                        Console.WriteLine("Kassett-ID, antal dygn och kvittonummer måste vara positiva heltal.");
                        break;
                    }

                    try
                    {
                        DateTime rentalDate = DateTime.Now;
                        DateTime dueDate = rentalDate.Date.AddDays(days);

                        if (_db.State == ConnectionState.Closed)
                        {
                            _db.Open();
                        }

                        using var transaction = _db.BeginTransaction();
                        int totalPrice;

                        try
                        {
                            var matchingCustomerIds = _db.Query<int>(
                                @"SELECT Id FROM Customer
                                  WHERE PersonNumber = @PersonNumber
                                  LIMIT 2 FOR UPDATE",
                                new { PersonNumber = rentalPersonNumber },
                                transaction: transaction).ToList();

                            if (matchingCustomerIds.Count == 0)
                            {
                                throw new InvalidOperationException("Ingen kund hittades med det personnumret.");
                            }

                            if (matchingCustomerIds.Count > 1)
                            {
                                throw new InvalidOperationException("Flera kunder har samma personnummer. Kontrollera kundregistret.");
                            }

                            var cassette = _db.QuerySingleOrDefault<CassetteForRental>(
                                @"SELECT fc.Id, f.IsNew
                                  FROM FilmCassette fc
                                  JOIN Film f ON f.Id = fc.FilmId
                                  WHERE fc.Id = @CassetteId
                                  FOR UPDATE",
                                new { CassetteId = cassetteId },
                                transaction: transaction);

                            if (cassette == null)
                            {
                                throw new InvalidOperationException("Kassetten hittades inte.");
                            }

                            int? activeRentalId = _db.QueryFirstOrDefault<int?>(
                                @"SELECT Id
                                  FROM RentFilm
                                  WHERE CassetteId = @CassetteId
                                    AND ReturnDate IS NULL
                                  LIMIT 1
                                  FOR UPDATE",
                                new { CassetteId = cassetteId },
                                transaction: transaction);

                            if (activeRentalId != null)
                            {
                                throw new InvalidOperationException("Kassetten är redan utlånad.");
                            }

                            int pricePerDay = cassette.IsNew ? 49 : 29;
                            totalPrice = checked(pricePerDay * days);

                            _db.Execute(
                                @"INSERT INTO Rental (CustomerId, RentalDateTime, ReceiptNumber)
                                  VALUES (@CustomerId, @RentalDateTime, @ReceiptNumber)",
                                new
                                {
                                    CustomerId = matchingCustomerIds[0],
                                    RentalDateTime = rentalDate,
                                    ReceiptNumber = receiptNumber
                                },
                                transaction: transaction);

                            long rentalId = _db.ExecuteScalar<long>(
                                "SELECT LAST_INSERT_ID()",
                                transaction: transaction);

                            _db.Execute(
                                @"INSERT INTO RentFilm
                                    (RentalId, CassetteId, DueDate, ReturnDate, RentalPrice)
                                  VALUES
                                    (@RentalId, @CassetteId, @DueDate, NULL, @RentalPrice)",
                                new
                                {
                                    RentalId = rentalId,
                                    CassetteId = cassetteId,
                                    DueDate = dueDate,
                                    RentalPrice = totalPrice
                                },
                                transaction: transaction);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }

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
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        Console.WriteLine($"Hyran kunde inte sparas på grund av ett databasfel: {ex.Message}");
                    }
                    break;
                }
                case "3":
                    Console.WriteLine("Ditt val: 3");
                    break;
                case "4":
                    Console.WriteLine("Ditt val: Lägg till kund");
                    Console.WriteLine("Lägg till namn (Förnamn Efternamn):");
                    string name = Console.ReadLine();
                    Console.WriteLine("Lägg till email (namn@email.com):");
                    string email = Console.ReadLine();
                    Console.WriteLine("Lägg till telefon (1234567890):");
                    string telephone = Console.ReadLine();
                    Console.WriteLine("Lägg till adress (Gatnamn 1, Staden):");
                    string address = Console.ReadLine();
                    Console.WriteLine("Lägg till personnummer (ÅÅÅÅMMDDxxxx):");
                    string personNumber = Console.ReadLine();

                    string sql = @"INSERT INTO Customer (Name, Email, Telephone, Address, PersonNumber)
                    VALUES (@Name, @Email, @Telephone, @Address, @PersonNumber)";

                    _db.Execute(sql, new { Name = name, Email = email, Telephone = telephone, Address = address, PersonNumber = personNumber });
                    break;
                case "5":
                    Console.WriteLine("Ditt val: Ta bort kund");
                    var customers = _db.Query<Customer>(
                        "SELECT Id, Name, Email, Telephone, Address, PersonNumber FROM Customer").ToList();

                    foreach (var customer in customers)
                    {
                        Console.WriteLine(
                            $"Id: {customer.Id}\nNamn: {customer.Name}\nE-post: {customer.Email}\nTelefon: {customer.Telephone}\nAdress: {customer.Address}\nPersonnummber: {customer.PersonNumber}\n\n");
                    }

                    Console.WriteLine("Vilken kund vill du ta bort? Id:");

                    if (int.TryParse(Console.ReadLine(), out int customerId))
                    {
                        string deleteCustomerSql = "DELETE FROM Customer WHERE Id = @Id";
                        int antalBorttagna = _db.Execute(deleteCustomerSql, new { Id = customerId });

                        Console.WriteLine(antalBorttagna == 1
                            ? "Kunden togs bort."
                            : "Ingen kund hittades med det ID:t.");
                    }
                    else
                    {
                        Console.WriteLine("Ange ett giltigt ID.");
                    }
                    break;
                case "6":
                    Console.WriteLine("Ditt val: 6");

                    var films = _db.Query<Film>(
                        "SELECT Id, Title, IsNew FROM Film"
                     );
                     foreach(var listedFilm in films)
                    {
                        string status = listedFilm.IsNew ? "Ny" : "Gammal";
                        Console.WriteLine($"Id: {listedFilm.Id}, Titel: {listedFilm.Title}, Status: {status}");
                    }

                    Console.WriteLine("Ange Id på filmen du vill ändra: ");

                    if(!int.TryParse(Console.ReadLine(),out int filmId))
                    {
                        Console.WriteLine("Du måste ange ett giltigt nummer.");
                        break;
                    }

                    var movie = _db.Query<Film>( 
                        "SELECT Id, Title, IsNew FROM Film WHERE Id = @Id",
                        new { Id = filmId}
                    ).FirstOrDefault();

                    if (movie == null)
                    {
                        Console.WriteLine("Filmen hittades inte.");
                        break;
                    }

                    Console.WriteLine($"Du har valt: {movie.Title}");
                    Console.WriteLine("1: Markera som ny");
                    Console.WriteLine("2: Markera som gammal");
                    string? val = Console.ReadLine();

                    if (val != "1" && val !="2")
                    {
                        Console.WriteLine("Ogiltigt val. Välj 1 eller 2");
                        break;
                    }

                    bool nyStatus = val == "1";

                    int result = _db.Execute(
                        "UPDATE Film SET IsNew = @IsNew WHERE Id = @Id",
                        new { IsNew = nyStatus, Id = movie.Id}
                     );

                     if (result > 0)
                    {
                        Console.WriteLine(
                            $"Filmen är nu märkt som: {(nyStatus ? "Ny" : "Gammal")}"
                         );
                    }
                    else
                    {
                        Console.WriteLine("Ingen rad uppdaterades; statusen kan redan vara vald.");
                    }
                    break;
                case "7":
                    Console.WriteLine("Filmer som inte är återlämnade:");

                    var filmer = _db.Query<string>(@"
                    SELECT DISTINCT f.Title
                    FROM RentFilm rf
                    JOIN FilmCassette fc ON fc.Id = rf.CassetteId
                    JOIN Film f ON f.Id = fc.FilmId
                    WHERE rf.ReturnDate is null");

                foreach (var film in filmer)
                {
                 Console.WriteLine(film);
                }
                    break;

                case "8":
                    Console.WriteLine("Förseningskostnad för återlämnade filmer:");

                    var senaFilmer = _db.Query<LateFilm>(@"
                    SELECT Film.Title, 
                    Film.IsNew, 
                    DATEDIFF(RentFilm.ReturnDate, 
                    RentFilm.DueDate) AS LateDays 
                    FROM RentFilm 
                    JOIN FilmCassette ON FilmCassette.Id = RentFilm.CassetteId 
                    JOIN Film ON Film.Id = FilmCassette.FilmId 
                    WHERE RentFilm.ReturnDate 
                    IS NOT NULL AND RentFilm.ReturnDate > RentFilm.DueDate");

                    foreach (var film in senaFilmer)
                    {
                        int avgiftPerDygn = film.IsNew ? 49 : 29;
                        int kostnad = film.LateDays * avgiftPerDygn;

                        Console.WriteLine($"{film.Title}: {film.LateDays} sena dygn, {kostnad} kr");
                    }
                    break;



                case "9":
                    Console.WriteLine("Programmet avslutas");
                    return;
                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }
        }
    }

    private class AvailableCassette
    {
        public int Id { get; set; }
        public string? Barcode { get; set; }
        public string? Title { get; set; }
        public bool IsNew { get; set; }
    }

    private class CassetteForRental
    {
        public int Id { get; set; }
        public bool IsNew { get; set; }
    }

    private class LateFilm
    {
        public string Title { get; set; }
        public bool IsNew { get; set; }
        public int LateDays { get; set; }
    }
}
