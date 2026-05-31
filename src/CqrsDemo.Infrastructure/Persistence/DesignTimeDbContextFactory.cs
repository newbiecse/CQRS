using CqrsDemo.Infrastructure.Persistence.Read;
using CqrsDemo.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CqrsDemo.Infrastructure.Persistence;

public sealed class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    public WriteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;database=CqrsDemo_Write;Trusted_Connection=False;persist security info=True;Integrated Security=SSPI;");

        return new WriteDbContext(optionsBuilder.Options);
    }
}

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
