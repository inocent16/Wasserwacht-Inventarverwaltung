using System.Text.Json;
using Wasserwacht.Api.Data;
using Wasserwacht.Api.Domain;

namespace Wasserwacht.Api.Infrastructure.AuditLog;

/// <summary>
/// Adds one complete action record to the central AuditLog table.
/// This writer deliberately does not call SaveChanges: the caller must persist
/// the domain changes and this record together through the same DbContext.
/// </summary>
public sealed class AuditLogWriter(InventoryDbContext dbContext)
{
    public AuditLogEntry Add(
        Guid actorUserId,
        string actorNameSnapshot,
        string eventType,
        JsonDocument? beforeState,
        JsonDocument? afterState)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException("An authenticated actor ID is required.", nameof(actorUserId));
        }

        var actorName = ValidateRequiredText(actorNameSnapshot, 200, nameof(actorNameSnapshot));
        var validatedEventType = ValidateRequiredText(eventType, 100, nameof(eventType));

        if (beforeState is null && afterState is null)
        {
            throw new ArgumentException(
                "At least one of beforeState or afterState must be provided.");
        }

        var entry = new AuditLogEntry
        {
            ActorUserId = actorUserId,
            ActorNameSnapshot = actorName,
            EventType = validatedEventType,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            BeforeState = CloneSnapshot(beforeState),
            AfterState = CloneSnapshot(afterState)
        };

        dbContext.AuditLog.Add(entry);
        return entry;
    }

    private static string ValidateRequiredText(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"The value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static JsonDocument? CloneSnapshot(JsonDocument? snapshot)
    {
        return snapshot is null
            ? null
            : JsonDocument.Parse(snapshot.RootElement.GetRawText());
    }
}
