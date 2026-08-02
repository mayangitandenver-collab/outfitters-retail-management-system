using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IInventoryIntelligenceApiClient
{
    Task<InventoryIntelligenceSummary> GetSummaryAsync(
        Guid? storeId,
        CancellationToken cancellationToken = default);

    Task<bool> CreatePurchaseRecommendationAsync(
        CreatePurchaseRecommendationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class InventoryIntelligenceApiClient
    : IInventoryIntelligenceApiClient
{
    private readonly HttpClient _httpClient;

    public InventoryIntelligenceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryIntelligenceSummary> GetSummaryAsync(
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        var path = storeId.HasValue
            ? $"/api/inventory/intelligence?storeId={storeId.Value}"
            : "/api/inventory/intelligence";

        try
        {
            return await _httpClient
                .GetFromJsonAsync<InventoryIntelligenceSummary>(
                    path,
                    cancellationToken)
                ?? new InventoryIntelligenceSummary();
        }
        catch
        {
            return new InventoryIntelligenceSummary();
        }
    }

    public async Task<bool> CreatePurchaseRecommendationAsync(
        CreatePurchaseRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/purchasing/recommendations",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
