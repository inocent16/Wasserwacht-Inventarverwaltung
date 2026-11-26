using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;
using Wasserwacht.Api.Domain;
using Wasserwacht.Api.Infrastructure.AuditLog;

namespace Wasserwacht.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/v1/auth");

        auth.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { requestToken = tokens.RequestToken });
        }).AllowAnonymous();

        auth.MapPost("/login", LoginAsync).AllowAnonymous().RequireRateLimiting("login");
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();
        auth.MapGet("/me", (ClaimsPrincipal principal) => Results.Ok(new
        {
            id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
            userName = principal.Identity?.Name,
            displayName = principal.FindFirstValue("display_name"),
            role = "Administrator"
        })).RequireAuthorization();

        auth.MapPost("/users", CreateUserAsync)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"));
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request, HttpContext context, IAntiforgery antiforgery,
        InventoryDbContext db, IPasswordHasher<InventoryUser> hasher)
    {
        await antiforgery.ValidateRequestAsync(context);
        var normalized = Normalize(request.UserName);
        if (normalized.Length == 0 || string.IsNullOrWhiteSpace(request.Password))
            return Results.Unauthorized();

        var user = await db.Users.SingleOrDefaultAsync(
            x => x.NormalizedUserName == normalized && x.IsActive, context.RequestAborted);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("display_name", user.DisplayName),
            new Claim(ClaimTypes.Role, "Administrator")
        };
        await context.SignInAsync("WasserwachtCookie",
            new ClaimsPrincipal(new ClaimsIdentity(claims, "WasserwachtCookie")),
            new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });
        return Results.Ok(new { user.Id, user.UserName, user.DisplayName, Role = "Administrator" });
    }

    private static async Task<IResult> LogoutAsync(HttpContext context, IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(context);
        await context.SignOutAsync("WasserwachtCookie");
        return Results.NoContent();
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request, HttpContext context, IAntiforgery antiforgery,
        InventoryDbContext db, IPasswordHasher<InventoryUser> hasher, AuditLogWriter audit)
    {
        await antiforgery.ValidateRequestAsync(context);
        var name = request.UserName?.Trim() ?? "";
        var display = request.DisplayName?.Trim() ?? "";
        var password = request.Password ?? "";
        if (name.Length is < 3 or > 100 || display.Length is < 1 or > 200 || password.Length < 14)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["userName"] = ["Benutzername muss 3 bis 100 Zeichen lang sein."],
                ["displayName"] = ["Anzeigename muss 1 bis 200 Zeichen lang sein."],
                ["password"] = ["Passwort muss mindestens 14 Zeichen lang sein."]
            });

        var normalized = Normalize(name);
        if (await db.Users.AnyAsync(x => x.NormalizedUserName == normalized, context.RequestAborted))
            return Results.Conflict(new { error = "Dieser Benutzername ist bereits vergeben." });

        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId))
            return Results.Unauthorized();

        var user = new InventoryUser
        {
            UserName = name, NormalizedUserName = normalized, DisplayName = display,
            PasswordHash = "pending"
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        await using var tx = await db.Database.BeginTransactionAsync(context.RequestAborted);
        db.Users.Add(user);
        using var after = JsonSerializer.SerializeToDocument(new
        {
            user.Id, user.UserName, user.DisplayName, Role = "Administrator", user.CreatedAtUtc
        });
        audit.Add(actorId, context.User.Identity?.Name ?? "unknown", "UserCreated", null, after);
        try
        {
            await db.SaveChangesAsync(context.RequestAborted);
            await tx.CommitAsync(context.RequestAborted);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(context.RequestAborted);
            return Results.Conflict(new { error = "Der Benutzername ist bereits vergeben." });
        }

        return Results.Created($"/api/v1/auth/users/{user.Id}",
            new { user.Id, user.UserName, user.DisplayName, Role = "Administrator" });
    }

    private static string Normalize(string? value) => (value ?? "").Trim().ToUpperInvariant();
}

public sealed record LoginRequest(string UserName, string Password);
public sealed record CreateUserRequest(string UserName, string DisplayName, string Password);
