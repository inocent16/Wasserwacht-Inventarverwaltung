using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;
using Wasserwacht.Api.Domain;
using Wasserwacht.Api.Features.Auth;
using Wasserwacht.Api.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Inventory")
    ?? Environment.GetEnvironmentVariable("WASSERWACHT_DB");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Database connection is missing. Configure ConnectionStrings:Inventory or WASSERWACHT_DB.");

if (args.Contains("--create-first-admin", StringComparer.Ordinal))
{
    await AdminBootstrapper.CreateFirstAdminAsync(connectionString);
    return;
}

builder.Services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<Wasserwacht.Api.Infrastructure.AuditLog.AuditLogWriter>();
builder.Services.AddScoped<IPasswordHasher<InventoryUser>, PasswordHasher<InventoryUser>>();

builder.Services.AddAuthentication("WasserwachtCookie").AddCookie("WasserwachtCookie", options =>
{
    options.Cookie.Name = "__Host-Wasserwacht.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-Wasserwacht.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
});
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapAuthEndpoints();
app.Run();

public partial class Program { }
