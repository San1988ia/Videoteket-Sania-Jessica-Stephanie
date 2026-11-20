using Dapper;
using MySql.Data.MySqlClient;

class FilmRepository
{
    private readonly string _connectionString;

    public FilmRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int Add(Film film)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Execute(
            @"INSERT INTO Film (Title, ReleaseDate, Description, IsActive, IsNew)
              VALUES (@Title, @ReleaseDate, @Description, @IsActive, @IsNew)",
            film);
    }

    public List<Film> GetAll()
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Query<Film>(
            "SELECT Id, Title, ReleaseDate, Description, IsActive, IsNew FROM Film").ToList();
    }

    public Film? GetById(int filmId)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.QuerySingleOrDefault<Film>(
            "SELECT Id, Title, IsNew FROM Film WHERE Id = @Id",
            new { Id = filmId });
    }

    public int UpdateStatus(int filmId, bool isNew)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Execute(
            "UPDATE Film SET IsNew = @IsNew WHERE Id = @Id",
            new { IsNew = isNew, Id = filmId });
    }

    public int DeleteById(int filmId)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Execute(
            "DELETE FROM Film WHERE Id = @Id",
            new { Id = filmId });
    }
}
