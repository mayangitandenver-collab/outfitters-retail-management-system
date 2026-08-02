namespace Outfitters.Web.Models;

public sealed class HrDashboardSummary
{
    public int ActiveEmployeeCount { get; set; }
    public int PresentTodayCount { get; set; }
    public int AbsentTodayCount { get; set; }
    public int OnLeaveTodayCount { get; set; }
    public int LateTodayCount { get; set; }
    public decimal CurrentMonthCommission { get; set; }

    public IReadOnlyCollection<EmployeePerformanceItem> TopEmployees { get; set; } =
        Array.Empty<EmployeePerformanceItem>();

    public IReadOnlyCollection<AttendanceAlertItem> AttendanceAlerts { get; set; } =
        Array.Empty<AttendanceAlertItem>();
}

public sealed class EmployeePerformanceItem
{
    public Guid EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal AverageTransactionValue { get; set; }
}

public sealed class AttendanceAlertItem
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class ClockEntryRequest
{
    public Guid EmployeeId { get; set; }
    public Guid StoreId { get; set; }
    public string Pin { get; set; } = string.Empty;
    public string EntryType { get; set; } = "ClockIn";
}

public sealed class ShiftAssignmentRequest
{
    public Guid EmployeeId { get; set; }
    public Guid StoreId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class LeaveRequestModel
{
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = "Vacation";
    public DateTime StartsOnUtc { get; set; }
    public DateTime EndsOnUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class PayrollExportRequest
{
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public Guid? StoreId { get; set; }
}
