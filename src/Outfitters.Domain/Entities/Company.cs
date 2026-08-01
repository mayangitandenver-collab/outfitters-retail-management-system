using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class Company : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Store> Stores { get; set; } = new List<Store>();
}
