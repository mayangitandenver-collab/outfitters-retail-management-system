using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Purchasing;
using Outfitters.Domain.Entities;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,InventoryClerk")]
[Route("api/purchase-orders")]
public sealed class PurchaseOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PurchaseOrdersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeId)
    {
        var query = _db.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Store)
            .AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(x => x.StoreId == storeId.Value);
        }

        var result = await query
            .OrderByDescending(x => x.OrderDateUtc)
            .Select(x => new
            {
                x.Id,
                x.PurchaseOrderNumber,
                Supplier = x.Supplier.Name,
                Store = x.Store.Name,
                x.OrderDateUtc,
                x.ExpectedDeliveryDateUtc,
                x.Status,
                x.GrandTotal
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseOrderRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one item is required.");
        }

        if (!await _db.Suppliers.AnyAsync(x => x.Id == request.SupplierId && x.IsActive))
        {
            return BadRequest("Supplier was not found.");
        }

        if (!await _db.Stores.AnyAsync(x => x.Id == request.StoreId && x.IsActive))
        {
            return BadRequest("Store was not found.");
        }

        var variantIds = request.Items.Select(x => x.ProductVariantId).Distinct().ToArray();
        if (variantIds.Length != request.Items.Count)
        {
            return BadRequest("Duplicate product variants are not allowed.");
        }

        var existingCount = await _db.ProductVariants.CountAsync(x =>
            variantIds.Contains(x.Id) && x.IsActive);

        if (existingCount != variantIds.Length)
        {
            return BadRequest("One or more product variants were not found.");
        }

        var po = new PurchaseOrder
        {
            PurchaseOrderNumber = await GeneratePurchaseOrderNumber(),
            SupplierId = request.SupplierId,
            StoreId = request.StoreId,
            CreatedByUserId = GetUserId(),
            ExpectedDeliveryDateUtc = request.ExpectedDeliveryDateUtc,
            Notes = request.Notes?.Trim(),
            Status = PurchaseOrderStatus.Submitted
        };

        foreach (var item in request.Items)
        {
            if (item.OrderedQuantity <= 0 || item.UnitCost < 0 ||
                item.DiscountAmount < 0 || item.TaxAmount < 0)
            {
                return BadRequest("Purchase-order values are invalid.");
            }

            var gross = item.OrderedQuantity * item.UnitCost;
            var total = gross - item.DiscountAmount + item.TaxAmount;

            po.Items.Add(new PurchaseOrderItem
            {
                ProductVariantId = item.ProductVariantId,
                OrderedQuantity = item.OrderedQuantity,
                UnitCost = item.UnitCost,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,
                LineTotal = total
            });
        }

        po.Subtotal = po.Items.Sum(x => x.OrderedQuantity * x.UnitCost);
        po.DiscountTotal = po.Items.Sum(x => x.DiscountAmount);
        po.TaxTotal = po.Items.Sum(x => x.TaxAmount);
        po.GrandTotal = po.Items.Sum(x => x.LineTotal);

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            po.Id,
            po.PurchaseOrderNumber,
            po.Status,
            po.GrandTotal
        });
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid id,
        ReceivePurchaseOrderRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one received item is required.");
        }

        var po = await _db.PurchaseOrders
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (po is null)
        {
            return NotFound();
        }

        if (po.Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.FullyReceived)
        {
            return BadRequest("This purchase order cannot receive more items.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var receipt = new GoodsReceipt
        {
            ReceiptNumber = await GenerateGoodsReceiptNumber(),
            PurchaseOrderId = po.Id,
            StoreId = po.StoreId,
            ReceivedByUserId = GetUserId(),
            SupplierInvoiceNumber = request.SupplierInvoiceNumber?.Trim(),
            Notes = request.Notes?.Trim()
        };

        foreach (var requested in request.Items)
        {
            var poItem = po.Items.SingleOrDefault(x => x.Id == requested.PurchaseOrderItemId);
            if (poItem is null)
            {
                return BadRequest("Purchase-order item was not found.");
            }

            var remaining = poItem.OrderedQuantity - poItem.ReceivedQuantity;
            if (requested.QuantityReceived <= 0 || requested.QuantityReceived > remaining)
            {
                return BadRequest("Received quantity is invalid.");
            }

            poItem.ReceivedQuantity += requested.QuantityReceived;

            receipt.Items.Add(new GoodsReceiptItem
            {
                PurchaseOrderItemId = poItem.Id,
                ProductVariantId = poItem.ProductVariantId,
                QuantityReceived = requested.QuantityReceived,
                UnitCost = requested.UnitCost
            });

            var inventory = await _db.InventoryItems.SingleOrDefaultAsync(x =>
                x.StoreId == po.StoreId &&
                x.ProductVariantId == poItem.ProductVariantId);

            if (inventory is null)
            {
                inventory = new InventoryItem
                {
                    StoreId = po.StoreId,
                    ProductVariantId = poItem.ProductVariantId
                };
                _db.InventoryItems.Add(inventory);
            }

            inventory.QuantityOnHand += requested.QuantityReceived;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            var variant = await _db.ProductVariants.SingleAsync(x =>
                x.Id == poItem.ProductVariantId);
            variant.CostPrice = requested.UnitCost;
            variant.UpdatedAtUtc = DateTime.UtcNow;

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                StoreId = po.StoreId,
                ProductVariantId = poItem.ProductVariantId,
                TransactionType = InventoryTransactionType.Purchase,
                QuantityChange = requested.QuantityReceived,
                BalanceAfter = inventory.QuantityOnHand,
                ReferenceNumber = receipt.ReceiptNumber,
                Remarks = $"Received against {po.PurchaseOrderNumber}",
                CreatedByUserId = receipt.ReceivedByUserId
            });
        }

        po.Status = po.Items.All(x => x.ReceivedQuantity >= x.OrderedQuantity)
            ? PurchaseOrderStatus.FullyReceived
            : PurchaseOrderStatus.PartiallyReceived;
        po.UpdatedAtUtc = DateTime.UtcNow;

        _db.GoodsReceipts.Add(receipt);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            receipt.Id,
            receipt.ReceiptNumber,
            po.PurchaseOrderNumber,
            po.Status,
            receipt.ReceivedAtUtc
        });
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("User identifier is missing.");
    }

    private async Task<string> GeneratePurchaseOrderNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var count = await _db.PurchaseOrders.CountAsync(x =>
            x.OrderDateUtc.Date == DateTime.UtcNow.Date);
        return $"PO-{datePart}-{count + 1:00000}";
    }

    private async Task<string> GenerateGoodsReceiptNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var count = await _db.GoodsReceipts.CountAsync(x =>
            x.ReceivedAtUtc.Date == DateTime.UtcNow.Date);
        return $"GR-{datePart}-{count + 1:00000}";
    }
}
