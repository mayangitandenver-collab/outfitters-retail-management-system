using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Inventory;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public InventoryController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid storeId)
    {
        var items = await _db.InventoryItems
            .AsNoTracking()
            .Where(x => x.StoreId == storeId)
            .Include(x => x.ProductVariant)
            .ThenInclude(x => x.Product)
            .OrderBy(x => x.ProductVariant.Product.Name)
            .Select(x => new
            {
                x.Id,
                x.StoreId,
                x.ProductVariantId,
                Product = x.ProductVariant.Product.Name,
                x.ProductVariant.VariantSku,
                x.ProductVariant.Barcode,
                x.ProductVariant.Size,
                x.ProductVariant.Color,
                x.QuantityOnHand,
                x.ReservedQuantity,
                AvailableQuantity = x.QuantityOnHand - x.ReservedQuantity,
                x.ReorderPoint,
                IsLowStock = x.QuantityOnHand - x.ReservedQuantity <= x.ReorderPoint
            })
            .ToListAsync();

        return Ok(items);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,InventoryClerk")]
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(AdjustInventoryRequest request)
    {
        if (!await _db.Stores.AnyAsync(x => x.Id == request.StoreId && x.IsActive))
        {
            return BadRequest("Store was not found.");
        }

        if (!await _db.ProductVariants.AnyAsync(x =>
            x.Id == request.ProductVariantId && x.IsActive))
        {
            return BadRequest("Product variant was not found.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var item = await _db.InventoryItems.SingleOrDefaultAsync(x =>
            x.StoreId == request.StoreId &&
            x.ProductVariantId == request.ProductVariantId);

        if (item is null)
        {
            item = new InventoryItem
            {
                StoreId = request.StoreId,
                ProductVariantId = request.ProductVariantId
            };
            _db.InventoryItems.Add(item);
        }

        var newBalance = item.QuantityOnHand + request.QuantityChange;
        if (newBalance < 0)
        {
            return BadRequest("This adjustment would create negative stock.");
        }

        item.QuantityOnHand = newBalance;
        item.UpdatedAtUtc = DateTime.UtcNow;

        Guid? userId = null;
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdText, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        _db.InventoryTransactions.Add(new InventoryTransaction
        {
            StoreId = request.StoreId,
            ProductVariantId = request.ProductVariantId,
            TransactionType = request.TransactionType,
            QuantityChange = request.QuantityChange,
            BalanceAfter = newBalance,
            ReferenceNumber = request.ReferenceNumber?.Trim(),
            Remarks = request.Remarks?.Trim(),
            CreatedByUserId = userId
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            item.StoreId,
            item.ProductVariantId,
            item.QuantityOnHand,
            item.ReservedQuantity,
            AvailableQuantity = item.QuantityOnHand - item.ReservedQuantity
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(
        [FromQuery] Guid storeId,
        [FromQuery] Guid? productVariantId)
    {
        var query = _db.InventoryTransactions
            .AsNoTracking()
            .Where(x => x.StoreId == storeId);

        if (productVariantId.HasValue)
        {
            query = query.Where(x => x.ProductVariantId == productVariantId.Value);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(500)
            .Select(x => new
            {
                x.Id,
                x.ProductVariantId,
                x.TransactionType,
                x.QuantityChange,
                x.BalanceAfter,
                x.ReferenceNumber,
                x.Remarks,
                x.CreatedAtUtc,
                x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(items);
    }
}
