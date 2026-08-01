using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class Customer : BaseEntity
{
    public string CustomerNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public Guid? CustomerTierId { get; set; }
    public CustomerTier? CustomerTier { get; set; }
    public decimal LoyaltyPointsBalance { get; set; }
    public decimal StoreCreditBalance { get; set; }
    public decimal LifetimeSpend { get; set; }
    public DateTime? LastPurchaseAtUtc { get; set; }
    public bool AcceptsEmailMarketing { get; set; }
    public bool AcceptsSmsMarketing { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } =
        new List<LoyaltyTransaction>();
    public ICollection<CustomerVoucher> Vouchers { get; set; } =
        new List<CustomerVoucher>();
    public ICollection<CustomerFavoriteProduct> FavoriteProducts { get; set; } =
        new List<CustomerFavoriteProduct>();
}
