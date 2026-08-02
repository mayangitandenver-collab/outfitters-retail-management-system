using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class EmployeeProfile : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid? PrimaryStoreId { get; set; }
    public Store? PrimaryStore { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public ICollection<EmployeeStoreAssignment> StoreAssignments { get; set; } = new List<EmployeeStoreAssignment>();
    public ICollection<EmployeeAttendance> AttendanceRecords { get; set; } = new List<EmployeeAttendance>();
}
