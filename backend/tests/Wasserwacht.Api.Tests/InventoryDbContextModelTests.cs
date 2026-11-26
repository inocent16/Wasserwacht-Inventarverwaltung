using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;
using Wasserwacht.Api.Domain;
using Wasserwacht.Api.Infrastructure.AuditLog;
using Xunit;

namespace Wasserwacht.Api.Tests;

public sealed class InventoryDbContextModelTests
{
    [Fact]
    public void StockItem_Quantity_IsARequiredInteger()
    {
        using var context = CreateContext();
        var entity = Assert.IsAssignableFrom<Microsoft.EntityFrameworkCore.Metadata.IEntityType>(
            context.Model.FindEntityType(typeof(StockItem)));
        var quantity = Assert.IsAssignableFrom<Microsoft.EntityFrameworkCore.Metadata.IProperty>(
            entity.FindProperty(nameof(StockItem.Quantity)));

        Assert.Equal(typeof(int), quantity.ClrType);
        Assert.False(quantity.IsNullable);
    }

    [Fact]
    public void StockItem_Quantity_HasPositiveDatabaseConstraint()
    {
        using var context = CreateContext();

        var createScript = context.Database.GenerateCreateScript();

        Assert.Contains("CK_StockItems_Quantity_Positive", createScript);
        Assert.Contains("\"Quantity\" > 0", createScript);
    }

    [Fact]
    public void StockItem_ExpirationBatchAndManufacturer_AreOptional()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entity);
        Assert.True(entity.FindProperty(nameof(StockItem.ExpirationDate))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(StockItem.BatchNumber))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(StockItem.Manufacturer))!.IsNullable);
    }

    [Fact]
    public void Article_RequiresNameAndCategory()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(Article));

        Assert.NotNull(entity);
        Assert.False(entity.FindProperty(nameof(Article.Name))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(Article.Category))!.IsNullable);
    }

    [Fact]
    public void AuditLog_IsTheOnlyHistoryTable()
    {
        using var context = CreateContext();

        var auditEntity = context.Model.FindEntityType(typeof(AuditLogEntry));

        Assert.NotNull(auditEntity);
        Assert.Equal("AuditLog", auditEntity.GetTableName());
        Assert.DoesNotContain(
            context.Model.GetEntityTypes(),
            entity => entity.ClrType.Name.Contains("Movement", StringComparison.OrdinalIgnoreCase));
        Assert.Single(context.Model.GetEntityTypes().Where(x => x.GetTableName() == "AuditLog"));
    }

    [Fact]
    public void InitialInventoryMigration_IsRegistered()
    {
        using var context = CreateContext();

        Assert.Contains("20261126203000_InitialInventorySchema", context.Database.GetMigrations());
    }

    [Fact]
    public void AuditLog_Snapshots_AreNullableJsonb()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(AuditLogEntry));

        Assert.NotNull(entity);
        Assert.True(entity.FindProperty(nameof(AuditLogEntry.BeforeState))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(AuditLogEntry.AfterState))!.IsNullable);
        Assert.Equal("jsonb", entity.FindProperty(nameof(AuditLogEntry.BeforeState))!.GetColumnType());
        Assert.Equal("jsonb", entity.FindProperty(nameof(AuditLogEntry.AfterState))!.GetColumnType());
    }

    [Fact]
    public void AuditLog_RequiresActorAndEventType()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(AuditLogEntry));

        Assert.NotNull(entity);
        Assert.False(entity.FindProperty(nameof(AuditLogEntry.ActorUserId))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(AuditLogEntry.ActorNameSnapshot))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(AuditLogEntry.EventType))!.IsNullable);
    }

    [Fact]
    public void AuditLogWriter_AddsOneCompleteActionRecord_WithoutSaving()
    {
        using var context = CreateContext();
        var writer = new AuditLogWriter(context);
        using var before = JsonDocument.Parse("""{"source":{"quantity":8},"target":{"quantity":2}}""");
        using var after = JsonDocument.Parse("""{"source":{"quantity":5},"target":{"quantity":5}}""");

        var entry = writer.Add(
            Guid.NewGuid(),
            "admin",
            "StockTransferred",
            before,
            after);

        Assert.Single(context.AuditLog.Local);
        Assert.Equal(EntityState.Added, context.Entry(entry).State);
        Assert.Equal("StockTransferred", entry.EventType);
        Assert.Equal("admin", entry.ActorNameSnapshot);
        Assert.Equal(8, entry.BeforeState!.RootElement.GetProperty("source").GetProperty("quantity").GetInt32());
        Assert.Equal(5, entry.AfterState!.RootElement.GetProperty("target").GetProperty("quantity").GetInt32());
    }

    [Fact]
    public void AuditLogWriter_ClonesSnapshots_AndDoesNotDependOnCallerLifetime()
    {
        using var context = CreateContext();
        var writer = new AuditLogWriter(context);
        var before = JsonDocument.Parse("""{"name":"old"}""");
        var after = JsonDocument.Parse("""{"name":"new"}""");

        var entry = writer.Add(Guid.NewGuid(), "admin", "ArticleUpdated", before, after);
        before.Dispose();
        after.Dispose();

        Assert.Equal("old", entry.BeforeState!.RootElement.GetProperty("name").GetString());
        Assert.Equal("new", entry.AfterState!.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public void AuditLogWriter_RejectsMissingActor()
    {
        using var context = CreateContext();
        var writer = new AuditLogWriter(context);
        using var after = JsonDocument.Parse("""{"name":"first"}""");

        Assert.Throws<ArgumentException>(() =>
            writer.Add(Guid.Empty, "admin", "ArticleCreated", null, after));

        Assert.Empty(context.AuditLog.Local);
    }

    [Fact]
    public void AuditLogWriter_RejectsEmptySnapshots()
    {
        using var context = CreateContext();
        var writer = new AuditLogWriter(context);

        Assert.Throws<ArgumentException>(() =>
            writer.Add(Guid.NewGuid(), "admin", "ArticleUpdated", null, null));

        Assert.Empty(context.AuditLog.Local);
    }


    [Fact]
    public void AuditLogEntries_CannotBeUpdated()
    {
        using var context = CreateContext();
        var existingEntry = new AuditLogEntry
        {
            ActorUserId = Guid.NewGuid(),
            ActorNameSnapshot = "admin",
            EventType = "ArticleCreated"
        };
        context.Attach(existingEntry);
        context.Entry(existingEntry).Property(x => x.EventType).IsModified = true;

        var exception = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

        Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AuditLogEntries_CannotBeDeleted()
    {
        using var context = CreateContext();
        var existingEntry = new AuditLogEntry
        {
            ActorUserId = Guid.NewGuid(),
            ActorNameSnapshot = "admin",
            EventType = "ArticleCreated"
        };
        context.Attach(existingEntry);
        context.Remove(existingEntry);

        var exception = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

        Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql("Host=localhost;Database=wasserwacht_model_tests;Username=test;Password=test")
            .Options;

        return new InventoryDbContext(options);
    }
}
