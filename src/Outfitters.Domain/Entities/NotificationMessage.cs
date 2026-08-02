using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class NotificationMessage : BaseEntity
{
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TemplateCode { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
