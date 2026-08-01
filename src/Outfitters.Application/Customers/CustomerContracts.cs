namespace Outfitters.Application.Customers;

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? BirthDate,
    string? Address,
    bool AcceptsEmailMarketing,
    bool AcceptsSmsMarketing);

public sealed record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? BirthDate,
    string? Address,
    Guid? CustomerTierId,
    bool AcceptsEmailMarketing,
    bool AcceptsSmsMarketing,
    bool IsActive);

public sealed record AdjustLoyaltyPointsRequest(
    decimal PointsChange,
    string? Notes);

public sealed record AdjustStoreCreditRequest(
    decimal AmountChange,
    string? Notes);

public sealed record CreateVoucherRequest(
    decimal DiscountAmount,
    decimal DiscountPercent,
    decimal MinimumSpend,
    DateTime ValidUntilUtc);

public sealed record AddFavoriteProductRequest(
    Guid ProductId);
