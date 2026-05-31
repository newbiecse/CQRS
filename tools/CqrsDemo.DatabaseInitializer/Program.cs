using Cart.Infrastructure.Persistence.Read;
using CheckoutSaga.Infrastructure.Persistence;
using CqrsDemo.BuildingBlocks.EventStore.Persistence;
using CqrsDemo.Commands.Infrastructure.Persistence.Write;
using CqrsDemo.Queries.Infrastructure.Persistence.Read;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Persistence.Read;
using Payment.Infrastructure.Persistence.Read;
using Product.Infrastructure.Persistence.Read;
using Reporting.Infrastructure.Persistence;
using User.Infrastructure.Persistence.Read;

const string connectionTemplate =
    "Server=localhost;database={0};Trusted_Connection=False;persist security info=True;Integrated Security=SSPI;TrustServerCertificate=True;";

var exportSql = args.Contains("--export-sql", StringComparer.OrdinalIgnoreCase);
var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var sqlOutputDir = Path.Combine(repoRoot, "scripts", "sql");

var targets = new (string Database, Func<string, DbContext> CreateContext)[]
{
    ("CqrsDemo_Product_Write", conn => Create<EventStoreDbContext>(conn)),
    ("CqrsDemo_Cart_Write", conn => Create<EventStoreDbContext>(conn)),
    ("CqrsDemo_Order_Write", conn => Create<EventStoreDbContext>(conn)),
    ("CqrsDemo_Payment_Write", conn => Create<EventStoreDbContext>(conn)),
    ("CqrsDemo_User_Write", conn => Create<EventStoreDbContext>(conn)),

    ("CqrsDemo_Product_Read", conn => Create<ProductReadDbContext>(conn)),
    ("CqrsDemo_Cart_Read", conn => Create<CartReadDbContext>(conn)),
    ("CqrsDemo_Order_Read", conn => Create<OrderReadDbContext>(conn)),
    ("CqrsDemo_Payment_Read", conn => Create<PaymentReadDbContext>(conn)),
    ("CqrsDemo_User_Read", conn => Create<UserReadDbContext>(conn)),

    ("CqrsDemo_Saga", conn => Create<CheckoutSagaDbContext>(conn)),
    ("CqrsDemo_Reporting", conn => Create<ReportingDbContext>(conn)),

    ("CqrsDemo_Write", conn => Create<WriteDbContext>(conn)),
    ("CqrsDemo_Read", conn => Create<ReadDbContext>(conn)),
};

Directory.CreateDirectory(sqlOutputDir);

foreach (var (database, createContext) in targets)
{
    var connection = string.Format(connectionTemplate, database);
    await using var context = createContext(connection);

    if (exportSql)
    {
        var script = context.Database.GenerateCreateScript();
        var scriptPath = Path.Combine(sqlOutputDir, $"{database}.sql");
        await File.WriteAllTextAsync(scriptPath, BuildSqlScript(database, script));
        Console.WriteLine($"Script: {scriptPath}");
    }

    var created = await context.Database.EnsureCreatedAsync();
    Console.WriteLine(created
        ? $"Created: {database}"
        : $"Exists:  {database}");
}

Console.WriteLine();
Console.WriteLine("All databases initialized.");

static TContext Create<TContext>(string connection) where TContext : DbContext
{
    var options = new DbContextOptionsBuilder<TContext>()
        .UseSqlServer(connection)
        .Options;
    return (TContext)Activator.CreateInstance(typeof(TContext), options)!;
}

static string BuildSqlScript(string database, string createTablesScript) =>
    $"""
    -- Auto-generated from EF Core models. Database: {database}
    IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{database}')
        CREATE DATABASE [{database}];
    GO

    USE [{database}];
    GO

    {createTablesScript}

    """;
