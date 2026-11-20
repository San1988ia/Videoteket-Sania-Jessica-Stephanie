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

    public int DeleteById(int filmId)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Execute(
            "DELETE FROM Film WHERE Id = @Id",
            new { Id = filmId });
    }
}
