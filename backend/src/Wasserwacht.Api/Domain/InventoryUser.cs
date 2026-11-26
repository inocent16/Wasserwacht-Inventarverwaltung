namespace Wasserwacht.Api.Domain;

/// <summary>Personal account. Version 1.0 has only the Administrator role.</summary>
public sealed class InventoryUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserName { get; set; }
    public required string NormalizedUserName { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
}
