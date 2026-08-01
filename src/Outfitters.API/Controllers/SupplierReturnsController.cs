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
[Route("api/supplier-returns")]
public sealed class SupplierReturnsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SupplierReturnsController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(CreateSupplierReturnRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one return item is required.");
        }

        if (!await _db.Suppliers.AnyAsync(x => x.Id == request.SupplierId && x.IsActive))
        {
            return BadRequest("Supplier was not found.");
        }

        if (!await _db.Stores.AnyAsync(x => x.Id == request.StoreId && x.IsActive))
        {
            return BadRequest("Store was not found.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var supplierReturn = new SupplierReturn
        {
            ReturnNumber = await GenerateReturnNumber(),
            SupplierId = request.SupplierId,
            StoreId = request.StoreId,
            ProcessedByUserId = GetUserId(),
            Status = SupplierReturnStatus.Completed,
            Reason = request.Reason?.Trim()
        };

        foreach (var requested in request.Items)
        {
            if (requested.Quantity <= 0 || requested.UnitCost < 0)
            {
                return BadRequest("Return quantity and cost are invalid.");
            }

            var inventory = await _db.InventoryItems.SingleOrDefaultAsync(x =>
                x.StoreId == request.StoreId &&
                x.ProductVariantId == requested.ProductVariantId);

            if (inventory is null || inventory.QuantityOnHand < requested.Quantity)
            {
                return BadRequest("Insufficient stock for supplier return.");
            }

            inventory.QuantityOnHand -= requested.Quantity;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            var lineTotal = requested.Quantity * requested.UnitCost;
            supplierReturn.TotalCost += lineTotal;
            supplierReturn.Items.Add(new SupplierReturnItem
            {
                ProductVariantId = requested.ProductVariantId,
                Quantity = requested.Quantity,
                UnitCost = requested.UnitCost,
                LineTotal = lineTotal
            });

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                StoreId = request.StoreId,
                ProductVariantId = requested.ProductVariantId,
                TransactionType = InventoryTransactionType.Adjustment,
                QuantityChange = -requested.Quantity,
                BalanceAfter = inventory.QuantityOnHand,
                ReferenceNumber = supplierReturn.ReturnNumber,
                Remarks = $"Supplier return: {request.Reason}",
                CreatedByUserId = supplierReturn.ProcessedByUserId
            });
        }

        _db.SupplierReturns.Add(supplierReturn);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            supplierReturn.Id,
            supplierReturn.ReturnNumber,
            supplierReturn.TotalCost,
            supplierReturn.Status
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
        var count = await _db.SupplierReturns.CountAsync(x =>
            x.ReturnDateUtc.Date == DateTime.UtcNow.Date);
        return $"SR-{datePart}-{count + 1:00000}";
    }
}
