using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using Dapper;
using Org.BouncyCastle.Math.EC;

class MainMenu
{
    private readonly IDbConnection _db;
    private readonly CustomerRepository _customerRepository;

    public MainMenu(IDbConnection db, CustomerRepository customerRepository)
    {
        _db = db;
        _customerRepository = customerRepository;
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
                    Console.WriteLine("Ditt val: Lägg till film");
                    Console.WriteLine("Lägg till titel:");
                    string title = Console.ReadLine();
                    Console.WriteLine("Lägg till utgivningsdatum (ÅÅÅÅ-MM-DD):");

                    if (!DateTime.TryParseExact(
                            Console.ReadLine(),
                            "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime releaseDate))
                    {
                        Console.WriteLine("Ogiltigt datum. Skriv till exempel 2026-11-20.");
                        break;
                    }
                    Console.WriteLine("Lägg till beskrivning:");
                    string description = Console.ReadLine();
                    Console.WriteLine("Är filmen ny? J/N");
                    string isNewString = Console.ReadLine().ToLower();
                    bool isNew;
                    if (isNewString == "j")
                    {
                        isNew = true;
                    }
                    else if (isNewString == "n")
                    {
                        isNew = false;
                    }
                    else
                    {
                        Console.WriteLine("Skriv J eller N.");
                        break;
                    }
                    bool isActive = true;

                    string addFilmSql = @"INSERT INTO Film (Title, ReleaseDate, Description, IsActive, IsNew)
                    VALUES (@Title, @ReleaseDate, @Description, @IsActive, @IsNew)";

                    int rowsAdded = _db.Execute(addFilmSql, new { Title = title, ReleaseDate = releaseDate, Description = description, IsActive = isActive, IsNew = isNew });
                    if (rowsAdded > 0)
                    {
                        Console.WriteLine("Filmen är tillagd.");
                    }
                    break;
                case "10":
                    Console.WriteLine("Ditt val: Ta bort film");
                    var removeFilms = _db.Query<Film>(
                        "SELECT Id, Title, ReleaseDate, Description, IsActive, IsNew FROM Film").ToList();

                    foreach (var film in removeFilms)
                    {
                        Console.WriteLine(
                            $"Id: {film.Id}\nTitel: {film.Title}\nUtgivningsdatum: {film.ReleaseDate}\nBeskrivning: {film.Description}\nAktiv: {(film.IsActive ? "Ja" : "Nej")}\nÅldersstatus: {(film.IsNew ? "Ny" : "Gammal")}\n\n");
                    }

                    Console.WriteLine("Vilken film vill du ta bort? Id:");

                    if (int.TryParse(Console.ReadLine(), out int removeFilmId))
                    {
                        string deleteFilmSql = "DELETE FROM Film WHERE Id = @Id";
                        int antalBorttagna = _db.Execute(deleteFilmSql, new { Id = removeFilmId });

                        Console.WriteLine(antalBorttagna == 1
                            ? "Filmen togs bort."
                            : "Ingen film hittades med det ID:t.");
                    }
                    else
                    {
                        Console.WriteLine("Ange ett giltigt ID.");
                    }
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

}
