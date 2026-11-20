using System.Data;
using System.Globalization;
using System.Security.Cryptography;
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
                    new ChangeFilmStatus(_filmRepository).Run();
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
