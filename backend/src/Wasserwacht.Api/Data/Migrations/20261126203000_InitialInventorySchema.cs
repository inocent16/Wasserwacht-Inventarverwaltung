using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wasserwacht.Api.Data;

#nullable disable

namespace Wasserwacht.Api.Data.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20261126203000_InitialInventorySchema")]
public partial class InitialInventorySchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Articles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Articles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Locations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Locations", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AuditLog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                ActorNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                BeforeState = table.Column<string>(type: "jsonb", nullable: true),
                AfterState = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AuditLog", x => x.Id));

        migrationBuilder.CreateTable(
            name: "StockItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                BatchNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StockItems", x => x.Id);
                table.CheckConstraint("CK_StockItems_Quantity_Positive", "\"Quantity\" > 0");
                table.ForeignKey(
                    name: "FK_StockItems_Articles_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Articles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StockItems_Locations_LocationId",
                    column: x => x.LocationId,
                    principalTable: "Locations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Name",
            table: "Articles",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_Locations_Name",
            table: "Locations",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StockItems_ArticleId_LocationId_ExpirationDate_BatchNumber",
            table: "StockItems",
            columns: new[] { "ArticleId", "LocationId", "ExpirationDate", "BatchNumber" });

        migrationBuilder.CreateIndex(
            name: "IX_StockItems_LocationId",
            table: "StockItems",
            column: "LocationId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLog_OccurredAtUtc",
            table: "AuditLog",
            column: "OccurredAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLog_ActorUserId",
            table: "AuditLog",
            column: "ActorUserId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLog_EventType",
            table: "AuditLog",
            column: "EventType");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLog");
        migrationBuilder.DropTable(name: "StockItems");
        migrationBuilder.DropTable(name: "Articles");
        migrationBuilder.DropTable(name: "Locations");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.15");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);

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
            entity.Property<string>("BeforeState").HasColumnType("jsonb");
            entity.Property<string>("AfterState").HasColumnType("jsonb");
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
