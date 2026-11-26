using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;
using Xunit;

namespace Wasserwacht.Api.Tests;

public sealed class InventoryDatabaseMigrationTests
{
    [Fact]
    public async Task InitialMigration_CreatesSchemaAndOnlyTheCentralAuditLogHistory()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("WASSERWACHT_DB")
            ?? throw new InvalidOperationException(
                "WASSERWACHT_DB must point to a disposable PostgreSQL database for migration integration tests.");

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new InventoryDbContext(options);
        await context.Database.MigrateAsync();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.Contains("20261126203000_InitialInventorySchema", appliedMigrations);

        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = """SELECT to_regclass('"AuditLog"') IS NOT NULL;""";
        Assert.True((bool)(await command.ExecuteScalarAsync())!);

        command.CommandText = """SELECT to_regclass('"StockMovements"') IS NULL;""";
        Assert.True((bool)(await command.ExecuteScalarAsync())!);

        command.CommandText = """
            SELECT data_type
            FROM information_schema.columns
            WHERE table_name = 'AuditLog' AND column_name = 'BeforeState';
            """;
        Assert.Equal("jsonb", await command.ExecuteScalarAsync());
    }
}
