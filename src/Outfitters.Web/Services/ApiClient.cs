using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IApiClient
{
    Task<LoginResult?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<DashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default);
}

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResult?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/login",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResult>(
            cancellationToken: cancellationToken);
    }

    public async Task<DashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<DashboardSummary>(
                "/api/reports/dashboard",
                cancellationToken);

            return result ?? CreateFallback();
        }
        catch
        {
            return CreateFallback();
        }
    }

    private static DashboardSummary CreateFallback()
    {
        return new DashboardSummary
        {
            TodaySales = 0m,
            MonthSales = 0m,
            InventoryValue = 0m,
            LowStockCount = 0,
            ActiveCustomers = 0,
            TransactionsToday = 0,
            SalesTrend = Array.Empty<DashboardMetric>(),
            TopProducts = Array.Empty<TopProductItem>()
        };
    }
}
