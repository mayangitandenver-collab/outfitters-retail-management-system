using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IHrApiClient
{
    Task<HrDashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default);

    Task<bool> PostClockEntryAsync(
        ClockEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CreateShiftAsync(
        ShiftAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CreateLeaveRequestAsync(
        LeaveRequestModel request,
        CancellationToken cancellationToken = default);

    Task<byte[]?> ExportPayrollAsync(
        PayrollExportRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class HrApiClient : IHrApiClient
{
    private readonly HttpClient _httpClient;

    public HrApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HrDashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient
                .GetFromJsonAsync<HrDashboardSummary>(
                    "/api/hr/dashboard",
                    cancellationToken)
                ?? new HrDashboardSummary();
        }
        catch
        {
            return new HrDashboardSummary();
        }
    }

    public async Task<bool> PostClockEntryAsync(
        ClockEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/hr/attendance/clock",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateShiftAsync(
        ShiftAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/hr/shifts",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateLeaveRequestAsync(
        LeaveRequestModel request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/hr/leave-requests",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<byte[]?> ExportPayrollAsync(
        PayrollExportRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/hr/payroll/export",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(
                cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
