using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class Supplier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
