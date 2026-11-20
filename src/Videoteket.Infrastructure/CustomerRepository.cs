using Dapper;
using MySql.Data.MySqlClient;

class CustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Customer> GetAll()
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Query<Customer>(
            "SELECT Id, Name, Email, Telephone, Address, PersonNumber FROM Customer").ToList();
    }

    public int DeleteById(int customerId)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Execute(
            "DELETE FROM Customer WHERE Id = @Id",
            new { Id = customerId });
    }

    public int Add(Customer customer)
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Execute(
            @"INSERT INTO Customer (Name, Email, Telephone, Address, PersonNumber)
              VALUES (@Name, @Email, @Telephone, @Address, @PersonNumber)",
            customer);
    }
}
