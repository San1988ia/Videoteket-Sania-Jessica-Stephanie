class DeleteFilm
{
    private readonly FilmRepository _filmRepository;

    public DeleteFilm(FilmRepository filmRepository)
    {
        _filmRepository = filmRepository;
    }

    public void Run()
    {
        Console.WriteLine("Ditt val: Ta bort film");
        var films = _filmRepository.GetAll();

        foreach (var film in films)
        {
            Console.WriteLine(
                $"Id: {film.Id}\nTitel: {film.Title}\nUtgivningsdatum: {film.ReleaseDate}\nBeskrivning: {film.Description}\nAktiv: {(film.IsActive ? "Ja" : "Nej")}\nÅldersstatus: {(film.IsNew ? "Ny" : "Gammal")}\n\n");
        }

        Console.WriteLine("Vilken film vill du ta bort? Id:");

        if (int.TryParse(Console.ReadLine(), out int filmId))
        {
            int rowsDeleted = _filmRepository.DeleteById(filmId);
            Console.WriteLine(rowsDeleted == 1
                ? "Filmen togs bort."
                : "Ingen film hittades med det ID:t.");
        }
        else
        {
            Console.WriteLine("Ange ett giltigt ID.");
        }
    }
}
