using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Catalog;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term) ||
                x.Sku.ToLower().Contains(term) ||
                x.Variants.Any(v =>
                    v.Barcode.ToLower().Contains(term) ||
                    v.VariantSku.ToLower().Contains(term)));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Sku,
                x.Name,
                Category = x.Category.Name,
                Brand = x.Brand == null ? null : x.Brand.Name,
                x.IsActive,
                Variants = x.Variants.Select(v => new
                {
                    v.Id,
                    v.VariantSku,
                    v.Barcode,
                    v.Size,
                    v.Color,
                    v.CostPrice,
                    v.SellingPrice,
                    v.IsActive
                })
            })
            .ToListAsync();

        return Ok(items);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,InventoryClerk")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        if (!await _db.Categories.AnyAsync(x => x.Id == request.CategoryId))
        {
            return BadRequest("Category was not found.");
        }

        if (request.BrandId.HasValue &&
            !await _db.Brands.AnyAsync(x => x.Id == request.BrandId.Value))
        {
            return BadRequest("Brand was not found.");
        }

        if (await _db.Products.AnyAsync(x => x.Sku == request.Sku))
        {
            return Conflict("Product SKU already exists.");
        }

        var variantSkus = request.Variants.Select(x => x.VariantSku).ToArray();
        var barcodes = request.Variants.Select(x => x.Barcode).ToArray();

        if (variantSkus.Distinct(StringComparer.OrdinalIgnoreCase).Count() != variantSkus.Length)
        {
            return BadRequest("Variant SKUs must be unique.");
        }

        if (barcodes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != barcodes.Length)
        {
            return BadRequest("Barcodes must be unique.");
        }

        if (await _db.ProductVariants.AnyAsync(x =>
            variantSkus.Contains(x.VariantSku) || barcodes.Contains(x.Barcode)))
        {
            return Conflict("A variant SKU or barcode already exists.");
        }

        var product = new Product
        {
            Sku = request.Sku.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Gender = request.Gender?.Trim(),
            Season = request.Season?.Trim(),
            Material = request.Material?.Trim(),
            Variants = request.Variants.Select(v => new ProductVariant
            {
                VariantSku = v.VariantSku.Trim(),
                Barcode = v.Barcode.Trim(),
                Size = v.Size?.Trim(),
                Color = v.Color?.Trim(),
                CostPrice = v.CostPrice,
                SellingPrice = v.SellingPrice
            }).ToList()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return Ok(new { product.Id, product.Sku, product.Name });
    }
}
