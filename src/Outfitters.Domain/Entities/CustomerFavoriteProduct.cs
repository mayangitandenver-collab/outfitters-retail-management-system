using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class CustomerFavoriteProduct : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
