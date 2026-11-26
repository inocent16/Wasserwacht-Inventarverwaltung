namespace Wasserwacht.Api.Domain;

public sealed class Article
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    // Initially free text. Existing values can later become managed category records.
    public required string Category { get; set; }
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}

public sealed class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}

public sealed class StockItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public int Quantity { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public string? Manufacturer { get; set; }
}
