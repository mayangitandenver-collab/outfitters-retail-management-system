using Outfitters.Domain.Enums;

namespace Outfitters.Application.Sales;

public sealed record OpenCashSessionRequest(
    Guid StoreId,
    decimal OpeningCash);

public sealed record CloseCashSessionRequest(
    decimal ClosingCash);

public sealed record CheckoutItemRequest(
    Guid ProductVariantId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount);

public sealed record CheckoutPaymentRequest(
    PaymentMethod Method,
    decimal Amount,
    string? ReferenceNumber);

public sealed record CheckoutRequest(
    Guid StoreId,
    Guid CashSessionId,
    IReadOnlyCollection<CheckoutItemRequest> Items,
    IReadOnlyCollection<CheckoutPaymentRequest> Payments,
    string? Notes);

public sealed record ReturnItemRequest(
    Guid SaleItemId,
    decimal Quantity,
    bool Restock);

public sealed record CreateReturnRequest(
    IReadOnlyCollection<ReturnItemRequest> Items,
    string? Reason);
