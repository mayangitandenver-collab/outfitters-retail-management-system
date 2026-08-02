using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public Guid? StoreId { get; set; }
    public Store? Store { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public Guid? PostedByUserId { get; set; }
    public ApplicationUser? PostedByUser { get; set; }
    public DateTime? PostedAtUtc { get; set; }
    public ICollection<JournalEntryLine> Lines { get; set; } =
        new List<JournalEntryLine>();
}
