using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class AccountsReceivable : BaseEntity
{
    public string ReceivableNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public DateTime InvoiceDateUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public ReceivableStatus Status { get; set; } = ReceivableStatus.Open;
}
