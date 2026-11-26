using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wasserwacht.Api.Data;

#nullable disable

namespace Wasserwacht.Api.Data.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20261126210000_AddInventoryUsers")]
public partial class AddInventoryUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                NormalizedUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Users_NormalizedUserName",
            table: "Users",
            column: "NormalizedUserName",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
            entity.Property<System.Text.Json.JsonDocument>("BeforeState").HasColumnType("jsonb");
            entity.Property<System.Text.Json.JsonDocument>("AfterState").HasColumnType("jsonb");
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
            entity.ToTable("StockItems", table => table.HasCheckConstraint("CK_StockItems_Quantity_Positive", "\"Quantity\" > 0"));
        });
        modelBuilder.Entity("Wasserwacht.Api.Domain.StockItem", entity =>
        {
            entity.HasOne("Wasserwacht.Api.Domain.Article", "Article").WithMany("StockItems").HasForeignKey("ArticleId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne("Wasserwacht.Api.Domain.Location", "Location").WithMany("StockItems").HasForeignKey("LocationId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.Navigation("Article");
            entity.Navigation("Location");
        });
        modelBuilder.Entity("Wasserwacht.Api.Domain.Article", entity => entity.Navigation("StockItems"));
        modelBuilder.Entity("Wasserwacht.Api.Domain.Location", entity => entity.Navigation("StockItems"));
    }
}
