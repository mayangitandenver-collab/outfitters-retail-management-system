using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class EmployeeAttendance : BaseEntity
{
    public Guid EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;
    public DateOnly WorkDate { get; set; }
    public DateTime? ClockInAtUtc { get; set; }
    public DateTime? ClockOutAtUtc { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
}
