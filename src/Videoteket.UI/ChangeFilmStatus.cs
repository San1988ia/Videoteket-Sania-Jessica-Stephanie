class ChangeFilmStatus
{
    private readonly FilmRepository _filmRepository;

    public ChangeFilmStatus(FilmRepository filmRepository)
    {
        _filmRepository = filmRepository;
    }

    public void Run()
    {
        Console.WriteLine("Ditt val: 6");

        foreach (var film in _filmRepository.GetAll())
        {
            string status = film.IsNew ? "Ny" : "Gammal";
            Console.WriteLine($"Id: {film.Id}, Titel: {film.Title}, Status: {status}");
        }

        Console.WriteLine("Ange Id på filmen du vill ändra: ");
        if (!int.TryParse(Console.ReadLine(), out int filmId))
        {
            Console.WriteLine("Du måste ange ett giltigt nummer.");
            return;
        }

        var filmToChange = _filmRepository.GetById(filmId);
        if (filmToChange == null)
        {
            Console.WriteLine("Filmen hittades inte.");
            return;
        }

        Console.WriteLine($"Du har valt: {filmToChange.Title}");
        Console.WriteLine("1: Markera som ny");
        Console.WriteLine("2: Markera som gammal");
        string? choice = Console.ReadLine();

        if (choice != "1" && choice != "2")
        {
            Console.WriteLine("Ogiltigt val. Välj 1 eller 2");
            return;
        }

        bool isNew = choice == "1";
        int result = _filmRepository.UpdateStatus(filmToChange.Id, isNew);

        if (result > 0)
        {
            Console.WriteLine($"Filmen är nu märkt som: {(isNew ? "Ny" : "Gammal")}");
        }
        else
        {
            Console.WriteLine("Ingen rad uppdaterades; statusen kan redan vara vald.");
        }
    }
}
