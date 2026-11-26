using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wasserwacht.Api.Data;

#nullable disable

namespace Wasserwacht.Api.Data.Migrations;

[DbContext(typeof(InventoryDbContext))]
public partial class InventoryDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.15");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("Wasserwacht.Api.Domain.InventoryUser", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd();
            entity.Property<string>("UserName").HasMaxLength(100).IsRequired();
            entity.Property<string>("NormalizedUserName").HasMaxLength(100).IsRequired();
            entity.Property<string>("DisplayName").HasMaxLength(200).IsRequired();
            entity.Property<string>("PasswordHash").HasMaxLength(1000).IsRequired();
            entity.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("timestamp with time zone");
            entity.Property<bool>("IsActive");
            entity.HasKey("Id");
            entity.HasIndex("NormalizedUserName").IsUnique();
            entity.ToTable("Users");
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.Article", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd();
            entity.Property<string>("Category").HasMaxLength(100).IsRequired();
            entity.Property<string>("Description").HasMaxLength(2000);
            entity.Property<string>("Manufacturer").HasMaxLength(200);
            entity.Property<string>("Name").HasMaxLength(200).IsRequired();
            entity.HasKey("Id");
            entity.HasIndex("Name");
            entity.ToTable("Articles");
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.Location", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd();
            entity.Property<string>("Description").HasMaxLength(2000);
            entity.Property<string>("Name").HasMaxLength(200).IsRequired();
            entity.HasKey("Id");
            entity.HasIndex("Name").IsUnique();
            entity.ToTable("Locations");
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.AuditLogEntry", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd();
            entity.Property<DateTimeOffset>("OccurredAtUtc").HasColumnType("timestamp with time zone");
            entity.Property<Guid>("ActorUserId");
            entity.Property<string>("ActorNameSnapshot").HasMaxLength(200).IsRequired();
            entity.Property<string>("EventType").HasMaxLength(100).IsRequired();
            entity.Property<JsonDocument>("BeforeState").HasColumnType("jsonb");
            entity.Property<JsonDocument>("AfterState").HasColumnType("jsonb");
            entity.HasKey("Id");
            entity.HasIndex("OccurredAtUtc");
            entity.HasIndex("ActorUserId");
            entity.HasIndex("EventType");
            entity.ToTable("AuditLog");
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.StockItem", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd();
            entity.Property<Guid>("ArticleId");
            entity.Property<Guid>("LocationId");
            entity.Property<int>("Quantity");
            entity.Property<DateOnly?>("ExpirationDate").HasColumnType("date");
            entity.Property<string>("BatchNumber").HasMaxLength(200);
            entity.Property<string>("Manufacturer").HasMaxLength(200);
            entity.HasKey("Id");
            entity.HasIndex("ArticleId", "LocationId", "ExpirationDate", "BatchNumber");
            entity.HasIndex("LocationId");
            entity.ToTable("StockItems", table =>
            {
                table.HasCheckConstraint("CK_StockItems_Quantity_Positive", "\"Quantity\" > 0");
            });
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.StockItem", entity =>
        {
            entity.HasOne("Wasserwacht.Api.Domain.Article", "Article")
                .WithMany("StockItems")
                .HasForeignKey("ArticleId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            entity.HasOne("Wasserwacht.Api.Domain.Location", "Location")
                .WithMany("StockItems")
                .HasForeignKey("LocationId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            entity.Navigation("Article");
            entity.Navigation("Location");
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.Article", entity =>
        {
            entity.Navigation("StockItems");
        });

        modelBuilder.Entity("Wasserwacht.Api.Domain.Location", entity =>
        {
            entity.Navigation("StockItems");
        });
    }
}
