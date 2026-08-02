using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class ExpenseRecord : BaseEntity
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public Guid ExpenseAccountId { get; set; }
    public GeneralLedgerAccount ExpenseAccount { get; set; } = null!;
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
