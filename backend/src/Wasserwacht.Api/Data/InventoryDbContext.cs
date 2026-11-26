using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Domain;

namespace Wasserwacht.Api.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Manufacturer).HasMaxLength(200);
            entity.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.ToTable("StockItems", table =>
                table.HasCheckConstraint("CK_StockItems_Quantity_Positive", "\"Quantity\" > 0"));
            entity.Property(x => x.BatchNumber).HasMaxLength(200);
            entity.Property(x => x.Manufacturer).HasMaxLength(200);
            entity.HasOne(x => x.Article).WithMany(x => x.StockItems)
                .HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Location).WithMany(x => x.StockItems)
                .HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ArticleId, x.LocationId, x.ExpirationDate, x.BatchNumber });
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.BatchNumber).HasMaxLength(200);
            entity.Property(x => x.Manufacturer).HasMaxLength(200);
            entity.Property(x => x.Note).HasMaxLength(2000);
            entity.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SourceLocation).WithMany()
                .HasForeignKey(x => x.SourceLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationLocation).WithMany()
                .HasForeignKey(x => x.DestinationLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.CreatedAt);
        });
    }
}
