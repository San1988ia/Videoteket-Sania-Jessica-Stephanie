using System.Data;
using Dapper;

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
                    Console.WriteLine("Ditt val: 5");
                    break;
                case "6":
                    Console.WriteLine("Ditt val: 6");
                    break;
                case "7":
                    Console.WriteLine("Ditt val: 7");
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