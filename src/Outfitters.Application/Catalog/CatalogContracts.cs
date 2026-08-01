namespace Outfitters.Application.Catalog;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId);

public sealed record CreateBrandRequest(
    string Name,
    string? Description);

public sealed record CreateProductVariantRequest(
    string VariantSku,
    string Barcode,
    string? Size,
    string? Color,
    decimal CostPrice,
    decimal SellingPrice);

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid? BrandId,
    string? Gender,
    string? Season,
    string? Material,
    IReadOnlyCollection<CreateProductVariantRequest> Variants);
