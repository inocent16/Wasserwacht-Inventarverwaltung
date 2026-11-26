using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;
using Wasserwacht.Api.Domain;
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

    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql("Host=localhost;Database=wasserwacht_model_tests;Username=test;Password=test")
            .Options;

        return new InventoryDbContext(options);
    }
}
