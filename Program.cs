using System.Data;
using MySql.Data.MySqlClient;
using Dapper;

DotNetEnv.Env.Load();

string connectionString =
    Environment.GetEnvironmentVariable("VIDEOTEKEt_CONNECTION_STRING")
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
