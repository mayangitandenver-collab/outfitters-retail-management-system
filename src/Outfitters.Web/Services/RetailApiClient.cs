using System.Net.Http.Json;
using Outfitters.Web.Models;

namespace Outfitters.Web.Services;

public interface IRetailApiClient
{
    Task<IReadOnlyCollection<PosProduct>> SearchProductsAsync(
        string query,
        Guid? storeId,
        CancellationToken cancellationToken = default);

    Task<SaleResult?> CreateSaleAsync(
        CreateSaleRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductListItem>> GetProductsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryListItem>> GetInventoryAsync(
        Guid? storeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CustomerListItem>> GetCustomersAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EmployeeListItem>> GetEmployeesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StoreListItem>> GetStoresAsync(
        CancellationToken cancellationToken = default);
}

public sealed class RetailApiClient : IRetailApiClient
{
    private readonly HttpClient _httpClient;

    public RetailApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<PosProduct>> SearchProductsAsync(
        string query,
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        var storePart = storeId.HasValue
            ? $"&storeId={storeId.Value}"
            : string.Empty;

        return await SafeGetAsync<PosProduct>(
            $"/api/catalog/search?query={Uri.EscapeDataString(query)}{storePart}",
            cancellationToken);
    }

    public async Task<SaleResult?> CreateSaleAsync(
        CreateSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/sales",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SaleResult>(
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyCollection<ProductListItem>> GetProductsAsync(
        CancellationToken cancellationToken = default) =>
        SafeGetAsync<ProductListItem>(
            "/api/products",
            cancellationToken);

    public Task<IReadOnlyCollection<InventoryListItem>> GetInventoryAsync(
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        var path = storeId.HasValue
            ? $"/api/inventory?storeId={storeId.Value}"
            : "/api/inventory";

        return SafeGetAsync<InventoryListItem>(
            path,
            cancellationToken);
    }

    public Task<IReadOnlyCollection<CustomerListItem>> GetCustomersAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(search)
            ? "/api/customers"
            : $"/api/customers?search={Uri.EscapeDataString(search)}";

        return SafeGetAsync<CustomerListItem>(
            path,
            cancellationToken);
    }

    public Task<IReadOnlyCollection<EmployeeListItem>> GetEmployeesAsync(
        CancellationToken cancellationToken = default) =>
        SafeGetAsync<EmployeeListItem>(
            "/api/employees",
            cancellationToken);

    public Task<IReadOnlyCollection<StoreListItem>> GetStoresAsync(
        CancellationToken cancellationToken = default) =>
        SafeGetAsync<StoreListItem>(
            "/api/stores",
            cancellationToken);

    private async Task<IReadOnlyCollection<T>> SafeGetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<T>>(
                path,
                cancellationToken);

            return result is null
                ? Array.Empty<T>()
                : result;
        }
        catch
        {
            return Array.Empty<T>();
        }
    }
}
