namespace Outfitters.Web.Models;

public sealed class PosProduct
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal AvailableQuantity { get; set; }
}

public sealed class PosCartItem
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; } = 1m;
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal =>
        Math.Max(0m, UnitPrice * Quantity - DiscountAmount);
}

public sealed class CheckoutPayment
{
    public int Method { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class CreateSaleRequest
{
    public Guid CheckoutId { get; set; }
    public Guid StoreId { get; set; }
    public Guid CashSessionId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyCollection<CreateSaleLineRequest> Items { get; set; } =
        Array.Empty<CreateSaleLineRequest>();
    public IReadOnlyCollection<CheckoutPayment> Payments { get; set; } =
        Array.Empty<CheckoutPayment>();
}

public sealed class CreateSaleLineRequest
{
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class SaleResult
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
}

public sealed class CashSessionListItem
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public decimal OpeningCash { get; set; }
    public int Status { get; set; }
}
public sealed class PosProductSearchItem
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IReadOnlyCollection<PosProductSearchVariant> Variants { get; set; }
        = Array.Empty<PosProductSearchVariant>();
}

public sealed class PosProductSearchVariant
{
    public Guid Id { get; set; }
    public string VariantSku { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public decimal SellingPrice { get; set; }
    public bool IsActive { get; set; }
}
