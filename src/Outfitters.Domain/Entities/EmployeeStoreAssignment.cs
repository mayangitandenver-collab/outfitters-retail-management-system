using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class EmployeeStoreAssignment : BaseEntity
{
    public Guid EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAtUtc { get; set; }
}
