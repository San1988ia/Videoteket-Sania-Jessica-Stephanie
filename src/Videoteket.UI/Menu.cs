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
            Console.WriteLine("8: Avsluta");

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
                    Console.WriteLine("Ditt val: 2");
                    break;
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
                    Console.WriteLine("Programmet avslutas");
                    return;
                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }
        }
    }
}

//Se filmer som inte är återlämnade