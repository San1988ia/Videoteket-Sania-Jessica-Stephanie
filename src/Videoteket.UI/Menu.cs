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
                    Console.WriteLine("Ditt val: 4");
                    break;
                case "5":
                    Console.WriteLine("Ditt val: 5");
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

                    var film = _db.Query<Film>( 
                        "SELECT Id, Title, IsNew FROM Film WHERE Id = @Id",
                        new { Id = filmId}
                    ).FirstOrDefault();

                    if (film == null)
                    {
                        Console.WriteLine("Filmen hittades inte.");
                        break;
                    }

                    Console.WriteLine($"Du har valt: {film.Title}");
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
                        new { IsNew = nyStatus, Id = film.Id}
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