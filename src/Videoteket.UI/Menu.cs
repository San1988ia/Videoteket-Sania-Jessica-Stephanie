using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using Dapper;
using Org.BouncyCastle.Math.EC;

class MainMenu
{
    private readonly IDbConnection _db;
    private readonly CustomerRepository _customerRepository;
    private readonly FilmRepository _filmRepository;
    private readonly RentalRepository _rentalRepository;

    public MainMenu(
        IDbConnection db,
        CustomerRepository customerRepository,
        FilmRepository filmRepository,
        RentalRepository rentalRepository)
    {
        _db = db;
        _customerRepository = customerRepository;
        _filmRepository = filmRepository;
        _rentalRepository = rentalRepository;
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
            Console.WriteLine("9: Lägg till film");
            Console.WriteLine("10: Ta bort film");
            Console.WriteLine("11: Avsluta");

            switch (Console.ReadLine())
            {
                case "1":
                    new ListFilms(_filmRepository).Run();
                    break;
                case "2":
                    new RentCassette(_rentalRepository).Run();
                    break;
                case "3":
                    Console.WriteLine("Ditt val: 3");
                    break;
                case "4":
                    new AddCustomer(_customerRepository).Run();
                    break;
                case "5":
                    new DeleteCustomer(_customerRepository).Run();
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
                    new ListUnreturnedFilms(_db).Run();
                    break;

                case "8":
                    new CalculateLateFees(_db).Run();
                    break;



                case "9":
                    new AddFilm(_filmRepository).Run();
                    break;
                case "10":
                    new DeleteFilm(_filmRepository).Run();
                    break;
                case "11":
                    Console.WriteLine("Programmet avslutas");
                    return;
                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }
        }
    }

    private class LateFilm
    {
        public string Title { get; set; }
        public bool IsNew { get; set; }
        public int LateDays { get; set; }
    }
}
