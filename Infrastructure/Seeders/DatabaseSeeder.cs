using Domain.Entities;
using Bogus;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.Seeders;

public class DatabaseSeeder
{
    private readonly string _connectionString;

    public DatabaseSeeder(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new ArgumentNullException("Connection string is missing!");
    }

    public async Task SeedOneMillionTransactionsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Transactions", connection);
        var count = (int)await checkCommand.ExecuteScalarAsync();

        if (count > 0)
        {
            Console.WriteLine($"Baza već ima {count} zapisa. Preskačem seeding.");
            return;
        }

        Console.WriteLine("Započinjem generiranje i upis 1.000.000 transakcija. Molim pričekajte...");

        // 2. Definiranje Bogus pravila za lažne podatke
        // Koristimo fiksni set Customer ID-jeva kako bismo imali ponavljajuće klijente za lakše pretraživanje
        var customerIds = Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToList();

        var transactionFaker = new Faker<Transaction>()
            .RuleFor(t => t.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(t => t.Amount, f => f.Finance.Amount(10m, 10000m))
            .RuleFor(t => t.TransactionDate, f => f.Date.Past(3)) // Transakcije unazad 3 godine
            .RuleFor(t => t.Status, f => f.PickRandom("Completed", "Pending", "Failed"))
            .RuleFor(t => t.Description, f => f.Commerce.ProductName());

        // 3. Generiranje i Bulk Insert u batchevima
        int totalRecords = 1_000_000;
        int batchSize = 100_000;

        for (int i = 0; i < totalRecords; i += batchSize)
        {
            var transactions = transactionFaker.Generate(batchSize);
            var dataTable = ConvertToDataTable(transactions);

            using var bulkCopy = new SqlBulkCopy(connection);
            bulkCopy.DestinationTableName = "Transactions";
            bulkCopy.BulkCopyTimeout = 120;

            // Mapiranje kolona (osigurava da se C# propertiji točno poklope sa SQL kolonama)
            bulkCopy.ColumnMappings.Add(nameof(Transaction.CustomerId), "CustomerId");
            bulkCopy.ColumnMappings.Add(nameof(Transaction.Amount), "Amount");
            bulkCopy.ColumnMappings.Add(nameof(Transaction.TransactionDate), "TransactionDate");
            bulkCopy.ColumnMappings.Add(nameof(Transaction.Status), "Status");
            bulkCopy.ColumnMappings.Add(nameof(Transaction.Description), "Description");

            await bulkCopy.WriteToServerAsync(dataTable);

            Console.WriteLine($"Uspješno upisano {i + batchSize} / {totalRecords} zapisa...");
        }

        Console.WriteLine("Seeding uspješno završen!");
    }

    // Pomoćna metoda za pretvaranje Liste u DataTable (koji SqlBulkCopy zahtijeva)
    private DataTable ConvertToDataTable(List<Transaction> transactions)
    {
        var table = new DataTable();
        table.Columns.Add("CustomerId", typeof(Guid));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("TransactionDate", typeof(DateTime));
        table.Columns.Add("Status", typeof(string));
        table.Columns.Add("Description", typeof(string));

        foreach (var t in transactions)
        {
            table.Rows.Add(t.CustomerId, t.Amount, t.TransactionDate, t.Status, t.Description);
        }
        return table;
    }
}