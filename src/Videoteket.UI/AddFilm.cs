using System.Globalization;

class AddFilm
{
    private readonly FilmRepository _filmRepository;

    public AddFilm(FilmRepository filmRepository)
    {
        _filmRepository = filmRepository;
    }

    public void Run()
    {
        Console.WriteLine("Ditt val: Lägg till film");
        Console.WriteLine("Lägg till titel:");
        string title = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Lägg till utgivningsdatum (ÅÅÅÅ-MM-DD):");

        if (!DateTime.TryParseExact(
                Console.ReadLine(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime releaseDate))
        {
            Console.WriteLine("Ogiltigt datum. Skriv till exempel 2026-11-20.");
            return;
        }

        Console.WriteLine("Lägg till beskrivning:");
        string description = Console.ReadLine() ?? string.Empty;
        Console.WriteLine("Är filmen ny? J/N");
        string isNewAnswer = Console.ReadLine()?.Trim() ?? string.Empty;

        bool isNew;
        if (string.Equals(isNewAnswer, "J", StringComparison.OrdinalIgnoreCase))
        {
            isNew = true;
        }
        else if (string.Equals(isNewAnswer, "N", StringComparison.OrdinalIgnoreCase))
        {
            isNew = false;
        }
        else
        {
            Console.WriteLine("Skriv J eller N.");
            return;
        }

        var film = new Film
        {
            Title = title,
            ReleaseDate = releaseDate,
            Description = description,
            IsActive = true,
            IsNew = isNew
        };

        int rowsAdded = _filmRepository.Add(film);
        Console.WriteLine(rowsAdded > 0
            ? "Filmen är tillagd."
            : "Filmen kunde inte läggas till.");
    }
}
