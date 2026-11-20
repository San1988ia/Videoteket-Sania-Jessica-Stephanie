class ListFilms
{
    private readonly FilmRepository _filmRepository;

    public ListFilms(FilmRepository filmRepository)
    {
        _filmRepository = filmRepository;
    }

    public void Run()
    {
        var films = _filmRepository.GetAll();
        foreach (var film in films)
        {
                Console.WriteLine(
                $"Id: {film.Id}\nTitel: {film.Title}\nUtgivningsdatum: {film.ReleaseDate}\nBeskrivning: {film.Description}\nAktiv: {(film.IsActive ? "Ja" : "Nej")}\nÅldersstatus: {(film.IsNew ? "Ny" : "Gammal")}\n\n");        }
    }
}
