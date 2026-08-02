using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public Guid GeneralLedgerAccountId { get; set; }
    public GeneralLedgerAccount GeneralLedgerAccount { get; set; } = null!;
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
