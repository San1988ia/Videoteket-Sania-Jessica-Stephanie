using System.Data;
using MySql.Data.MySqlClient;

DotNetEnv.Env.Load();

string connectionString =
    Environment.GetEnvironmentVariable("VIDEOTEKET_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Miljövariabeln VIDEOTEKET_CONNECTION_STRING saknas.");
using IDbConnection db = new MySqlConnection(connectionString);

var customerRepository = new CustomerRepository(connectionString);
var filmRepository = new FilmRepository(connectionString);
var rentalRepository = new RentalRepository(connectionString);
var menu = new MainMenu(db, customerRepository, filmRepository, rentalRepository);
menu.Run();
