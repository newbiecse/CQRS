using CqrsDemo.Queries.Infrastructure.Persistence.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CqrsDemo.Queries.Infrastructure.Persistence;

public sealed class ReadDbContextFactory : IDesignTimeDbContextFactory<ReadDbContext>
{
    public ReadDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReadDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=CqrsDemo_Read;User Id=sa;Password=Your_password123;TrustServerCertificate=True;");

        return new ReadDbContext(optionsBuilder.Options);
    }
}
