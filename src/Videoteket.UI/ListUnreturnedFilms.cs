using System.Data;
using Dapper;

class ListUnreturnedFilms
{
    private readonly IDbConnection _db;

    public ListUnreturnedFilms(IDbConnection db)
    {
        _db = db;
    }

    public void Run()
    {
        Console.WriteLine("Filmer som inte är återlämnade:");

        var filmer = _db.Query<string>(@"
            SELECT DISTINCT f.Title
            FROM RentFilm rf
            JOIN FilmCassette fc ON fc.Id = rf.CassetteId
            JOIN Film f ON f.Id = fc.FilmId
            WHERE rf.ReturnDate IS NULL");

        foreach (var film in filmer)
        {
            Console.WriteLine(film);
        }
    }
}
