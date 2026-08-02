using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IPurchasingApiClient
{
    Task<PurchasingDashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default);

    Task<bool> CreatePurchaseOrderAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ReceivePurchaseOrderAsync(
        GoodsReceiptRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ApprovePurchaseOrderAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default);
}

public sealed class PurchasingApiClient : IPurchasingApiClient
{
    private readonly HttpClient _httpClient;

    public PurchasingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PurchasingDashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient
                .GetFromJsonAsync<PurchasingDashboardSummary>(
                    "/api/purchasing/dashboard",
                    cancellationToken)
                ?? new PurchasingDashboardSummary();
        }
        catch
        {
            return new PurchasingDashboardSummary();
        }
    }

    public async Task<bool> CreatePurchaseOrderAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/purchase-orders",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReceivePurchaseOrderAsync(
        GoodsReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/purchase-orders/{request.PurchaseOrderId}/receive",
            request,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApprovePurchaseOrderAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            $"/api/purchase-orders/{purchaseOrderId}/approve",
            content: null,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
