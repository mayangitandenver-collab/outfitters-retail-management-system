using System.Net.Http.Json;
using Outfitters.Web.Models;
using System.Net.Http.Headers;
using Outfitters.Web.Authentication;

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

    Task<IReadOnlyCollection<CashSessionListItem>> GetOpenCashSessionsAsync(
    Guid storeId,
    CancellationToken cancellationToken = default);

Task<string?> GetReceiptAsync(
    Guid saleId,
    CancellationToken cancellationToken = default);
}
public sealed class RetailApiClient : IRetailApiClient
{
    private readonly HttpClient _httpClient;
private readonly AuthSession _session;

public RetailApiClient(HttpClient httpClient, AuthSession session)
{
    _httpClient = httpClient;
    _session = session;
}
    private void ApplyAuthorization()
{
    _httpClient.DefaultRequestHeaders.Authorization =
        string.IsNullOrWhiteSpace(_session.AccessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", _session.AccessToken);
}

    public async Task<IReadOnlyCollection<PosProduct>> SearchProductsAsync(
    string query,
    Guid? storeId,
    CancellationToken cancellationToken = default)
{
    ApplyAuthorization();

    var products = await SafeGetAsync<PosProductSearchItem>(
        $"/api/products?search={Uri.EscapeDataString(query)}",
        cancellationToken);

    var inventory = await GetInventoryAsync(
        storeId,
        cancellationToken);

    var inventoryByVariantId = inventory
        .ToDictionary(
            x => x.ProductVariantId,
            x => x.QuantityOnHand);

    var results = products
        .SelectMany(product => product.Variants
            .Where(variant => variant.IsActive)
            .Select(variant =>
            {
                inventoryByVariantId.TryGetValue(
                    variant.Id,
                    out var availableQuantity);

                var variantName = string.Join(
                    " / ",
                    new[] { variant.Color, variant.Size }
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                return new PosProduct
                {
                    ProductVariantId = variant.Id,
                    ProductName = product.Name,
                    VariantName = string.IsNullOrWhiteSpace(variantName)
                        ? variant.VariantSku
                        : variantName,
                    Barcode = variant.Barcode,
                    UnitPrice = variant.SellingPrice,
                    AvailableQuantity = availableQuantity
                };
            }))
        .ToArray();

    return results;
}

    public async Task<SaleResult?> CreateSaleAsync(
        CreateSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        ApplyAuthorization();

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/sales/checkout",
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
    CancellationToken cancellationToken = default)
{
    ApplyAuthorization();

    return SafeGetAsync<ProductListItem>(
        "/api/products",
        cancellationToken);
}

    public Task<IReadOnlyCollection<InventoryListItem>> GetInventoryAsync(
        Guid? storeId,
        CancellationToken cancellationToken = default)
    {
        ApplyAuthorization();

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
        ApplyAuthorization();

        var path = string.IsNullOrWhiteSpace(search)
            ? "/api/customers"
            : $"/api/customers?search={Uri.EscapeDataString(search)}";

        return SafeGetAsync<CustomerListItem>(
            path,
            cancellationToken);
    }

        public Task<IReadOnlyCollection<EmployeeListItem>> GetEmployeesAsync(
    CancellationToken cancellationToken = default)
{
    ApplyAuthorization();

    return SafeGetAsync<EmployeeListItem>(
        "/api/employees",
        cancellationToken);
}

    public Task<IReadOnlyCollection<StoreListItem>> GetStoresAsync(
    CancellationToken cancellationToken = default)
{
    ApplyAuthorization();

    return SafeGetAsync<StoreListItem>(
        "/api/stores",
        cancellationToken);
}
    public Task<IReadOnlyCollection<CashSessionListItem>> GetOpenCashSessionsAsync(
    Guid storeId,
    CancellationToken cancellationToken = default)
{
    ApplyAuthorization();

    return SafeGetAsync<CashSessionListItem>(
        $"/api/cash-sessions/open?storeId={storeId}",
        cancellationToken);
}

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
public async Task<string?> GetReceiptAsync(
    Guid saleId,
    CancellationToken cancellationToken = default)
{
    ApplyAuthorization();

    using var response = await _httpClient.GetAsync(
        $"/api/sales/{saleId}/receipt",
        cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        return null;
    }

    return await response.Content.ReadAsStringAsync(cancellationToken);
}
}
