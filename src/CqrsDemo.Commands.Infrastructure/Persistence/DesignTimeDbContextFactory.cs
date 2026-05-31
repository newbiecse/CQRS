using CqrsDemo.Commands.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CqrsDemo.Commands.Infrastructure.Persistence;

public sealed class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    public WriteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=CqrsDemo_Write;User Id=sa;Password=Your_password123;TrustServerCertificate=True;");

        return new WriteDbContext(optionsBuilder.Options);
    }
}
