using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface ICrmApiClient
{
    Task<CrmDashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default);

    Task<bool> AdjustLoyaltyAsync(
        LoyaltyAdjustmentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> AdjustStoreCreditAsync(
        StoreCreditAdjustmentRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CreateVoucherAsync(
        CustomerVoucherRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CrmApiClient : ICrmApiClient
{
    private readonly HttpClient _httpClient;

    public CrmApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CrmDashboardSummary> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient
                .GetFromJsonAsync<CrmDashboardSummary>(
                    "/api/customer-reports/dashboard",
                    cancellationToken)
                ?? new CrmDashboardSummary();
        }
        catch
        {
            return new CrmDashboardSummary();
        }
    }

    public async Task<bool> AdjustLoyaltyAsync(
        LoyaltyAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/customers/{request.CustomerId}/loyalty/adjust",
            new
            {
                request.PointsChange,
                request.Notes
            },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AdjustStoreCreditAsync(
        StoreCreditAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/customers/{request.CustomerId}/store-credit/adjust",
            new
            {
                request.AmountChange,
                request.Notes
            },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateVoucherAsync(
        CustomerVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/customers/{request.CustomerId}/vouchers",
            new
            {
                request.DiscountAmount,
                request.DiscountPercent,
                request.MinimumSpend,
                request.ValidUntilUtc
            },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
