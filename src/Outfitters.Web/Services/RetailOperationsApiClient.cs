using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IRetailOperationsApiClient
{
    Task<bool> CreateStockTransferAsync(
        StockTransferRequest request,
        CancellationToken cancellationToken = default);

    Task<GiftCardLookupResult?> LookupGiftCardAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<SalesAnalyticsSummary> GetSalesAnalyticsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}

public sealed class RetailOperationsApiClient
    : IRetailOperationsApiClient
{
    private readonly HttpClient _httpClient;

    public RetailOperationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CreateStockTransferAsync(
        StockTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/stock-transfers",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<GiftCardLookupResult?> LookupGiftCardAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GiftCardLookupResult>(
                $"/api/gift-cards/{Uri.EscapeDataString(code)}",
                cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<SalesAnalyticsSummary> GetSalesAnalyticsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var path =
            $"/api/reports/sales-analytics?from={from:yyyy-MM-dd}" +
            $"&to={to:yyyy-MM-dd}";

        try
        {
            return await _httpClient
                .GetFromJsonAsync<SalesAnalyticsSummary>(
                    path,
                    cancellationToken)
                ?? new SalesAnalyticsSummary();
        }
        catch
        {
            return new SalesAnalyticsSummary();
        }
    }
}
