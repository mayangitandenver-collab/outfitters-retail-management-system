using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class CustomerVoucher : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal MinimumSpend { get; set; }
    public DateTime ValidFromUtc { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntilUtc { get; set; }
    public VoucherStatus Status { get; set; } = VoucherStatus.Active;
    public DateTime? RedeemedAtUtc { get; set; }
    public Guid? RedeemedSaleId { get; set; }
    public Sale? RedeemedSale { get; set; }
}
