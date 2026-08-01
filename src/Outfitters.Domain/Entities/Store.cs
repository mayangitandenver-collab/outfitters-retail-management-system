using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class Store : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsMainStore { get; set; }
    public bool IsActive { get; set; } = true;
}
