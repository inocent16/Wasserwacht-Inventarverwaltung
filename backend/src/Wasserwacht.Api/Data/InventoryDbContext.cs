using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Domain;

namespace Wasserwacht.Api.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();
    public DbSet<InventoryUser> Users => Set<InventoryUser>();

    public override int SaveChanges()
    {
        EnsureAuditLogIsAppendOnly();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureAuditLogIsAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuditLogIsAppendOnly();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureAuditLogIsAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureAuditLogIsAppendOnly()
    {
        var changedEntry = ChangeTracker.Entries<AuditLogEntry>()
            .FirstOrDefault(entry =>
                entry.State is EntityState.Modified or EntityState.Deleted);

        if (changedEntry is not null)
        {
            throw new InvalidOperationException(
                "AuditLog entries are append-only and cannot be updated or deleted.");
        }
    }

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

        modelBuilder.Entity<InventoryUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NormalizedUserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => x.NormalizedUserName).IsUnique();
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.ToTable("AuditLog");
            entity.Property(x => x.ActorNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.BeforeState).HasColumnType("jsonb");
            entity.Property(x => x.AfterState).HasColumnType("jsonb");

            // Deliberately no foreign-key cascade to mutable/deletable domain records.
            // ActorUserId is retained as an immutable historical identifier.
            entity.HasIndex(x => x.OccurredAtUtc);
            entity.HasIndex(x => x.ActorUserId);
            entity.HasIndex(x => x.EventType);
        });
    }
}
