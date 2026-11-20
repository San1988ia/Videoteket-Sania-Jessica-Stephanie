using Dapper;
using MySql.Data.MySqlClient;

class RentalRepository
{
    private readonly string _connectionString;

    public RentalRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<AvailableCassette> GetAvailableCassettes()
    {
        using var db = new MySqlConnection(_connectionString);
        return db.Query<AvailableCassette>(
            @"SELECT fc.Id, fc.Barcode, f.Title, f.IsNew
              FROM FilmCassette fc
              JOIN Film f ON f.Id = fc.FilmId
              WHERE NOT EXISTS (
                  SELECT 1
                  FROM RentFilm rf
                  WHERE rf.CassetteId = fc.Id
                    AND rf.ReturnDate IS NULL
              )
              ORDER BY f.Title, fc.Id").ToList();
    }

    public int RentCassette(
        string personNumber, int cassetteId, int days, int receiptNumber,
        DateTime rentalDate, DateTime dueDate)
    {
        using var db = new MySqlConnection(_connectionString);
        db.Open();
        using var transaction = db.BeginTransaction();

        try
        {
            var matchingCustomerIds = db.Query<int>(
                @"SELECT Id FROM Customer
                  WHERE PersonNumber = @PersonNumber
                  LIMIT 2 FOR UPDATE",
                new { PersonNumber = personNumber },
                transaction: transaction).ToList();

            if (matchingCustomerIds.Count == 0)
            {
                throw new InvalidOperationException("Ingen kund hittades med det personnumret.");
            }

            if (matchingCustomerIds.Count > 1)
            {
                throw new InvalidOperationException("Flera kunder har samma personnummer. Kontrollera kundregistret.");
            }

            var cassette = db.QuerySingleOrDefault<CassetteForRental>(
                @"SELECT fc.Id, f.IsNew
                  FROM FilmCassette fc
                  JOIN Film f ON f.Id = fc.FilmId
                  WHERE fc.Id = @CassetteId
                  FOR UPDATE",
                new { CassetteId = cassetteId },
                transaction: transaction);

            if (cassette == null)
            {
                throw new InvalidOperationException("Kassetten hittades inte.");
            }

            int? activeRentalId = db.QueryFirstOrDefault<int?>(
                @"SELECT Id
                  FROM RentFilm
                  WHERE CassetteId = @CassetteId
                    AND ReturnDate IS NULL
                  LIMIT 1
                  FOR UPDATE",
                new { CassetteId = cassetteId },
                transaction: transaction);

            if (activeRentalId != null)
            {
                throw new InvalidOperationException("Kassetten är redan utlånad.");
            }

            int pricePerDay = RentalPricing.GetPricePerDay(cassette.IsNew);
            int totalPrice = checked(pricePerDay * days);

            db.Execute(
                @"INSERT INTO Rental (CustomerId, RentalDateTime, ReceiptNumber)
                  VALUES (@CustomerId, @RentalDateTime, @ReceiptNumber)",
                new
                {
                    CustomerId = matchingCustomerIds[0],
                    RentalDateTime = rentalDate,
                    ReceiptNumber = receiptNumber
                },
                transaction: transaction);

            long rentalId = db.ExecuteScalar<long>(
                "SELECT LAST_INSERT_ID()",
                transaction: transaction);

            db.Execute(
                @"INSERT INTO RentFilm
                    (RentalId, CassetteId, DueDate, ReturnDate, RentalPrice)
                  VALUES
                    (@RentalId, @CassetteId, @DueDate, NULL, @RentalPrice)",
                new
                {
                    RentalId = rentalId,
                    CassetteId = cassetteId,
                    DueDate = dueDate,
                    RentalPrice = totalPrice
                },
                transaction: transaction);

            transaction.Commit();
            return totalPrice;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private class CassetteForRental
    {
        public int Id { get; set; }
        public bool IsNew { get; set; }
    }
}
