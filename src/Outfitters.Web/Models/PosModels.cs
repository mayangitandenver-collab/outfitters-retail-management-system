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
    public string Method { get; set; } = "Cash";
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class CreateSaleRequest
{
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
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
}

public sealed class SaleResult
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
}
