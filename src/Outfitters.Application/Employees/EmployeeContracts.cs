using Outfitters.Domain.Enums;

namespace Outfitters.Application.Employees;

public sealed record CreateEmployeeRequest(
    string Email, string FirstName, string LastName,
    string TemporaryPassword, string JobTitle, DateOnly HireDate,
    Guid? PrimaryStoreId, IReadOnlyCollection<string> Roles,
    string? EmergencyContactName, string? EmergencyContactPhone);

public sealed record UpdateEmployeeRequest(
    string FirstName, string LastName, string JobTitle,
    Guid? PrimaryStoreId, EmployeeStatus Status,
    string? EmergencyContactName, string? EmergencyContactPhone);

public sealed record AssignRolesRequest(IReadOnlyCollection<string> Roles);
public sealed record AssignStoreRequest(Guid StoreId, bool IsPrimary);
public sealed record ResetEmployeePasswordRequest(string NewPassword);
public sealed record RecordAttendanceRequest(
    DateOnly WorkDate, DateTime? ClockInAtUtc, DateTime? ClockOutAtUtc,
    AttendanceStatus Status, string? Notes);
