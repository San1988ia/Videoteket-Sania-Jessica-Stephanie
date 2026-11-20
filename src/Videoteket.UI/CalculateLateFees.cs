using System.Data;
using Dapper;

class CalculateLateFees
{
    private readonly IDbConnection _db;

    public CalculateLateFees(IDbConnection db)
    {
        _db = db;
    }

    public void Run()
    {
        Console.WriteLine("Förseningskostnad för återlämnade filmer:");

        var senaFilmer = _db.Query<LateFilm>(@"
            SELECT Film.Title,
                   Film.IsNew,
                   DATEDIFF(RentFilm.ReturnDate, RentFilm.DueDate) AS LateDays
            FROM RentFilm
            JOIN FilmCassette ON FilmCassette.Id = RentFilm.CassetteId
            JOIN Film ON Film.Id = FilmCassette.FilmId
            WHERE RentFilm.ReturnDate IS NOT NULL
              AND RentFilm.ReturnDate > RentFilm.DueDate");

        foreach (var film in senaFilmer)
        {
            int avgiftPerDygn = film.IsNew ? 49 : 29;
            int kostnad = film.LateDays * avgiftPerDygn;

            Console.WriteLine($"{film.Title}: {film.LateDays} sena dygn, {kostnad} kr");
        }
    }

    private class LateFilm
    {
        public string Title { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public int LateDays { get; set; }
    }
}
