using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Inventory")
    ?? Environment.GetEnvironmentVariable("WASSERWACHT_DB")
    ?? "Host=localhost;Port=5432;Database=wasserwacht_inventory;Username=postgres;Password=postgres";

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
