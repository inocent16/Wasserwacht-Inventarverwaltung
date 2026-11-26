using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Wasserwacht.Api.Data;

/// <summary>
/// Creates the context for EF Core tooling without starting the web host.
/// Configure ConnectionStrings__Inventory or WASSERWACHT_DB before using dotnet ef.
/// </summary>
public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Inventory")
            ?? Environment.GetEnvironmentVariable("WASSERWACHT_DB");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__Inventory or WASSERWACHT_DB before running EF Core tooling.");
        }

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new InventoryDbContext(options);
    }
}
