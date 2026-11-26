using System.Text.Json;

namespace Wasserwacht.Api.Domain;

/// <summary>
/// The single, central history record for one completed application action.
/// BeforeState and AfterState contain all affected data for the whole action.
/// </summary>
public sealed class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Guid ActorUserId { get; set; }

    public required string ActorNameSnapshot { get; set; }

    public required string EventType { get; set; }

    public JsonDocument? BeforeState { get; set; }

    public JsonDocument? AfterState { get; set; }
}
