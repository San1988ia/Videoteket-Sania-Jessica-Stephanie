using System.Data;
using MySql.Data.MySqlClient;
using Dapper;

DotNetEnv.Env.Load();

string connectionString =
    Environment.GetEnvironmentVariable("VIDEOTEKET_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Miljövariabeln VIDEOTEKET_CONNECTION_STRING saknas.");
using IDbConnection db = new MySqlConnection(connectionString);

// Väldigt basic meny, vi kan utveckla den framöver

while (true)
{
Console.WriteLine("Hej! Välkommen till Videoteket.");
Console.WriteLine("Välj ditt menyval nedan:");
Console.WriteLine("1: Se utbud");
Console.WriteLine("2: Låna film");
Console.WriteLine("3: Återlämna film");
Console.WriteLine("4: Avsluta");

// Lägg till felhantering också

switch (Console.ReadLine())
{
    case "1":
{
    var titlar = db.Query<string>("SELECT Title FROM Film");

    foreach (var titel in titlar)
    {
        Console.WriteLine(titel);
    }

    break;
}
                case "2":
            {
               Console.WriteLine("Ditt val: 2");
               break; 
            }
                case "3":
            {
               Console.WriteLine("Ditt val: 3");
               break; 
            }

                case "4":
            {
                Console.WriteLine("Ditt val: 4");
                Console.WriteLine("Programmet avslutas");
                return;
            }
    
    default:
    break;
}
}

class Film
{
    public int Id {get; set;}
    public string Title {get; set;}
    public DateTime ReleaseDate {get; set;}
    public string Description {get; set;}
    public bool IsActive {get; set;}
    public bool IsNew {get; set;}
}

class FilmCassette
{
    public int Id {get; set;}
    public int FilmId {get; set;}
    public string Barcode {get; set;}
    public DateTime EditionDate {get; set;}
}

class Rental
{
    public int Id {get; set;}
    public int CustomerId {get; set;}
    public DateTime RentalDateTime {get; set;}
    public int ReceiptNumber {get; set;}
}

class Customer
{
    public int Id {get; set;}
    public string Name {get; set;} 
    public string Email {get; set;}
    public string Telephone {get; set;}
    public string Address {get; set;}
    public string PersonNumber {get; set;}
}