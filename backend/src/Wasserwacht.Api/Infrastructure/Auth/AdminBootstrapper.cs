using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wasserwacht.Api.Data;
using Wasserwacht.Api.Domain;
using Wasserwacht.Api.Infrastructure.AuditLog;

namespace Wasserwacht.Api.Infrastructure.Auth;

/// <summary>Interactive, one-time first administrator provisioning.</summary>
public static class AdminBootstrapper
{
    public static async Task CreateFirstAdminAsync(string connectionString)
    {
        if (Console.IsInputRedirected)
            throw new InvalidOperationException("Die Admin-Ersteinrichtung muss interaktiv in einem Terminal laufen.");

        var options = new DbContextOptionsBuilder<InventoryDbContext>().UseNpgsql(connectionString).Options;
        await using var db = new InventoryDbContext(options);
        await db.Database.MigrateAsync();

        Console.Write("Admin-Benutzername: ");
        var userName = (Console.ReadLine() ?? "").Trim();
        Console.Write("Anzeigename: ");
        var displayName = (Console.ReadLine() ?? "").Trim();
        if (userName.Length is < 3 or > 100 || displayName.Length is < 1 or > 200)
            throw new InvalidOperationException("Benutzername oder Anzeigename hat eine ungültige Länge.");

        var password = ReadPassword("Passwort (mindestens 14 Zeichen): ");
        var confirmation = ReadPassword("Passwort wiederholen: ");
        if (password.Length < 14 || !string.Equals(password, confirmation, StringComparison.Ordinal))
            throw new InvalidOperationException("Passwörter stimmen nicht überein oder sind zu kurz.");

        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (await db.Users.AnyAsync())
            throw new InvalidOperationException("Es existiert bereits ein Benutzerkonto. Die Ersteinrichtung ist nur einmal möglich.");

        var user = new InventoryUser
        {
            UserName = userName, NormalizedUserName = userName.ToUpperInvariant(),
            DisplayName = displayName, PasswordHash = "pending"
        };
        user.PasswordHash = new PasswordHasher<InventoryUser>().HashPassword(user, password);
        db.Users.Add(user);

        using var after = JsonSerializer.SerializeToDocument(new
        {
            user.Id, user.UserName, user.DisplayName, Role = "Administrator",
            user.CreatedAtUtc, Provisioning = "InitialAdministrator"
        });
        new AuditLogWriter(db).Add(user.Id, user.UserName, "InitialAdministratorCreated", null, after);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        Console.WriteLine("Der erste Administrator wurde eingerichtet.");
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var chars = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return new string(chars.ToArray()); }
            if (key.Key == ConsoleKey.Backspace) { if (chars.Count > 0) chars.RemoveAt(chars.Count - 1); continue; }
            if (!char.IsControl(key.KeyChar)) chars.Add(key.KeyChar);
        }
    }
}
