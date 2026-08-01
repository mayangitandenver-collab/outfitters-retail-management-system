using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Sales;
using Outfitters.Domain.Entities;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
[Route("api/sales/{saleId:guid}/returns")]
public sealed class ReturnsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReturnsController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(Guid saleId, CreateReturnRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one return item is required.");
        }

        var sale = await _db.Sales
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == saleId);

        if (sale is null)
        {
            return NotFound();
        }

        if (sale.Status == SaleStatus.Voided)
        {
            return BadRequest("Voided sales cannot be returned.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var saleReturn = new SaleReturn
        {
            SaleId = saleId,
            ProcessedByUserId = GetUserId(),
            ReturnNumber = await GenerateReturnNumber(),
            Reason = request.Reason?.Trim()
        };

        foreach (var requestedItem in request.Items)
        {
            var saleItem = sale.Items.SingleOrDefault(x => x.Id == requestedItem.SaleItemId);
            if (saleItem is null)
            {
                return BadRequest($"Sale item {requestedItem.SaleItemId} was not found.");
            }

            var remainingReturnable = saleItem.Quantity - saleItem.ReturnedQuantity;
            if (requestedItem.Quantity <= 0 ||
                requestedItem.Quantity > remainingReturnable)
            {
                return BadRequest("Return quantity is invalid.");
            }

            var unitRefund = saleItem.LineTotal / saleItem.Quantity;
            var refundAmount = decimal.Round(
                unitRefund * requestedItem.Quantity,
                2,
                MidpointRounding.AwayFromZero);

            saleReturn.Items.Add(new SaleReturnItem
            {
                SaleItemId = saleItem.Id,
                Quantity = requestedItem.Quantity,
                RefundAmount = refundAmount,
                Restock = requestedItem.Restock
            });

            saleItem.ReturnedQuantity += requestedItem.Quantity;
            saleReturn.RefundAmount += refundAmount;

            if (requestedItem.Restock)
            {
                var inventory = await _db.InventoryItems.SingleOrDefaultAsync(x =>
                    x.StoreId == sale.StoreId &&
                    x.ProductVariantId == saleItem.ProductVariantId);

                if (inventory is null)
                {
                    inventory = new InventoryItem
                    {
                        StoreId = sale.StoreId,
                        ProductVariantId = saleItem.ProductVariantId
                    };
                    _db.InventoryItems.Add(inventory);
                }

                inventory.QuantityOnHand += requestedItem.Quantity;
                inventory.UpdatedAtUtc = DateTime.UtcNow;

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    StoreId = sale.StoreId,
                    ProductVariantId = saleItem.ProductVariantId,
                    TransactionType = InventoryTransactionType.Return,
                    QuantityChange = requestedItem.Quantity,
                    BalanceAfter = inventory.QuantityOnHand,
                    ReferenceNumber = saleReturn.ReturnNumber,
                    Remarks = request.Reason?.Trim(),
                    CreatedByUserId = saleReturn.ProcessedByUserId
                });
            }
        }

        var allReturned = sale.Items.All(x => x.ReturnedQuantity >= x.Quantity);
        sale.Status = allReturned
            ? SaleStatus.FullyReturned
            : SaleStatus.PartiallyReturned;
        sale.UpdatedAtUtc = DateTime.UtcNow;

        _db.SaleReturns.Add(saleReturn);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            saleReturn.Id,
            saleReturn.ReturnNumber,
            saleReturn.RefundAmount,
            SaleStatus = sale.Status,
            saleReturn.CreatedAtUtc
        });
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("User identifier is missing.");
    }

    private async Task<string> GenerateReturnNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var countToday = await _db.SaleReturns.CountAsync(x =>
            x.CreatedAtUtc.Date == DateTime.UtcNow.Date);

        return $"RT-{datePart}-{countToday + 1:00000}";
    }
}
