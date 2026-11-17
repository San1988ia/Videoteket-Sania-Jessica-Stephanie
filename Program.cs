using System.Data;
using MySql.Data.MySqlClient;

DotNetEnv.Env.Load();

string connectionString =
    Environment.GetEnvironmentVariable("VIDEOTEKET_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Miljövariabeln VIDEOTEKET_CONNECTION_STRING saknas.");
using IDbConnection db = new MySqlConnection(connectionString);

var menu = new MainMenu(db);
menu.Run();
