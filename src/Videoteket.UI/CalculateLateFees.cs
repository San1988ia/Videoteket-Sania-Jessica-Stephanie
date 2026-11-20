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

        var senaFilmer = _db.Query<LateFilm>(
            "CalculateLateFees",
            commandType: CommandType.StoredProcedure);
            

        foreach (var film in senaFilmer)
        {
        

            Console.WriteLine($"{film.Title}: {film.LateDays} sena dygn, {film.Cost} kr");
        }
    }

    private class LateFilm
    {
        public string Title { get; set; } = string.Empty;
        public int LateDays { get; set; }
        public int Cost {get; set; }
    }
}
