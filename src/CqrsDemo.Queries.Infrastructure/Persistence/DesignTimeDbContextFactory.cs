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
            "Server=localhost;database=CqrsDemo_Read;Trusted_Connection=False;persist security info=True;Integrated Security=SSPI;");

        return new ReadDbContext(optionsBuilder.Options);
    }
}
