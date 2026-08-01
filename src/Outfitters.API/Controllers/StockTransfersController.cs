using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Transfers;
using Outfitters.Domain.Entities;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,InventoryClerk")]
[Route("api/stock-transfers")]
public sealed class StockTransfersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StockTransfersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? sourceStoreId,
        [FromQuery] Guid? destinationStoreId,
        [FromQuery] StockTransferStatus? status)
    {
        var query = _db.StockTransfers
            .AsNoTracking()
            .Include(x => x.SourceStore)
            .Include(x => x.DestinationStore)
            .AsQueryable();

        if (sourceStoreId.HasValue)
        {
            query = query.Where(x => x.SourceStoreId == sourceStoreId.Value);
        }

        if (destinationStoreId.HasValue)
        {
            query = query.Where(x => x.DestinationStoreId == destinationStoreId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var result = await query
            .OrderByDescending(x => x.TransferDateUtc)
            .Select(x => new
            {
                x.Id,
                x.TransferNumber,
                SourceStore = x.SourceStore.Name,
                DestinationStore = x.DestinationStore.Name,
                x.TransferDateUtc,
                x.DispatchedAtUtc,
                x.ReceivedAtUtc,
                x.Status
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _db.StockTransfers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.TransferNumber,
                x.SourceStoreId,
                x.DestinationStoreId,
                x.Status,
                x.TransferDateUtc,
                x.DispatchedAtUtc,
                x.ReceivedAtUtc,
                x.Notes,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ProductVariantId,
                    i.RequestedQuantity,
                    i.DispatchedQuantity,
                    i.ReceivedQuantity,
                    i.DamagedQuantity,
                    InTransitQuantity =
                        i.DispatchedQuantity - i.ReceivedQuantity - i.DamagedQuantity
                })
            })
            .SingleOrDefaultAsync();

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStockTransferRequest request)
    {
        if (request.SourceStoreId == request.DestinationStoreId)
        {
            return BadRequest("Source and destination stores must be different.");
        }

        if (request.Items.Count == 0)
        {
            return BadRequest("At least one transfer item is required.");
        }

        var storeCount = await _db.Stores.CountAsync(x =>
            (x.Id == request.SourceStoreId || x.Id == request.DestinationStoreId) &&
            x.IsActive);

        if (storeCount != 2)
        {
            return BadRequest("One or both stores were not found.");
        }

        var variantIds = request.Items.Select(x => x.ProductVariantId).Distinct().ToArray();
        if (variantIds.Length != request.Items.Count)
        {
            return BadRequest("Duplicate product variants are not allowed.");
        }

        var transfer = new StockTransfer
        {
            TransferNumber = await GenerateTransferNumber(),
            SourceStoreId = request.SourceStoreId,
            DestinationStoreId = request.DestinationStoreId,
            CreatedByUserId = GetUserId(),
            Status = StockTransferStatus.Submitted,
            Notes = request.Notes?.Trim()
        };

        foreach (var item in request.Items)
        {
            if (item.RequestedQuantity <= 0)
            {
                return BadRequest("Requested quantity must be greater than zero.");
            }

            if (!await _db.ProductVariants.AnyAsync(x =>
                x.Id == item.ProductVariantId && x.IsActive))
            {
                return BadRequest("A product variant was not found.");
            }

            transfer.Items.Add(new StockTransferItem
            {
                ProductVariantId = item.ProductVariantId,
                RequestedQuantity = item.RequestedQuantity
            });
        }

        _db.StockTransfers.Add(transfer);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            transfer.Id,
            transfer.TransferNumber,
            transfer.Status
        });
    }

    [HttpPost("{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(
        Guid id,
        DispatchStockTransferRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one dispatch item is required.");
        }

        var transfer = await _db.StockTransfers
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (transfer is null)
        {
            return NotFound();
        }

        if (transfer.Status is StockTransferStatus.Cancelled or
            StockTransferStatus.InTransit or
            StockTransferStatus.PartiallyReceived or
            StockTransferStatus.FullyReceived)
        {
            return BadRequest("This transfer cannot be dispatched.");
        }

        await using var dbTransaction = await _db.Database.BeginTransactionAsync();

        foreach (var requested in request.Items)
        {
            var transferItem = transfer.Items.SingleOrDefault(x =>
                x.Id == requested.StockTransferItemId);

            if (transferItem is null)
            {
                return BadRequest("Transfer item was not found.");
            }

            if (requested.QuantityToDispatch <= 0 ||
                requested.QuantityToDispatch > transferItem.RequestedQuantity)
            {
                return BadRequest("Dispatch quantity is invalid.");
            }

            var sourceInventory = await _db.InventoryItems.SingleOrDefaultAsync(x =>
                x.StoreId == transfer.SourceStoreId &&
                x.ProductVariantId == transferItem.ProductVariantId);

            if (sourceInventory is null ||
                sourceInventory.QuantityOnHand - sourceInventory.ReservedQuantity <
                requested.QuantityToDispatch)
            {
                return BadRequest("Insufficient source-store inventory.");
            }

            sourceInventory.QuantityOnHand -= requested.QuantityToDispatch;
            sourceInventory.UpdatedAtUtc = DateTime.UtcNow;
            transferItem.DispatchedQuantity = requested.QuantityToDispatch;

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                StoreId = transfer.SourceStoreId,
                ProductVariantId = transferItem.ProductVariantId,
                TransactionType = InventoryTransactionType.TransferOut,
                QuantityChange = -requested.QuantityToDispatch,
                BalanceAfter = sourceInventory.QuantityOnHand,
                ReferenceNumber = transfer.TransferNumber,
                Remarks = $"Transfer to store {transfer.DestinationStoreId}",
                CreatedByUserId = GetUserId()
            });
        }

        transfer.Status = StockTransferStatus.InTransit;
        transfer.DispatchedAtUtc = DateTime.UtcNow;
        transfer.DispatchedByUserId = GetUserId();
        transfer.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? transfer.Notes
            : request.Notes.Trim();
        transfer.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        return Ok(new
        {
            transfer.Id,
            transfer.TransferNumber,
            transfer.Status,
            transfer.DispatchedAtUtc
        });
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid id,
        ReceiveStockTransferRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one receipt item is required.");
        }

        var transfer = await _db.StockTransfers
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (transfer is null)
        {
            return NotFound();
        }

        if (transfer.Status is not
            (StockTransferStatus.InTransit or StockTransferStatus.PartiallyReceived))
        {
            return BadRequest("This transfer is not available for receiving.");
        }

        await using var dbTransaction = await _db.Database.BeginTransactionAsync();

        var receipt = new StockTransferReceipt
        {
            ReceiptNumber = await GenerateReceiptNumber(),
            StockTransferId = transfer.Id,
            ReceivedByUserId = GetUserId(),
            Notes = request.Notes?.Trim()
        };

        foreach (var requested in request.Items)
        {
            var transferItem = transfer.Items.SingleOrDefault(x =>
                x.Id == requested.StockTransferItemId);

            if (transferItem is null)
            {
                return BadRequest("Transfer item was not found.");
            }

            if (requested.QuantityReceived < 0 || requested.QuantityDamaged < 0)
            {
                return BadRequest("Received and damaged quantities cannot be negative.");
            }

            var remaining = transferItem.DispatchedQuantity -
                transferItem.ReceivedQuantity -
                transferItem.DamagedQuantity;

            var processed = requested.QuantityReceived + requested.QuantityDamaged;
            if (processed <= 0 || processed > remaining)
            {
                return BadRequest("Received quantity exceeds in-transit quantity.");
            }

            transferItem.ReceivedQuantity += requested.QuantityReceived;
            transferItem.DamagedQuantity += requested.QuantityDamaged;

            receipt.Items.Add(new StockTransferReceiptItem
            {
                StockTransferItemId = transferItem.Id,
                ProductVariantId = transferItem.ProductVariantId,
                QuantityReceived = requested.QuantityReceived,
                QuantityDamaged = requested.QuantityDamaged
            });

            if (requested.QuantityReceived > 0)
            {
                var destinationInventory = await _db.InventoryItems.SingleOrDefaultAsync(x =>
                    x.StoreId == transfer.DestinationStoreId &&
                    x.ProductVariantId == transferItem.ProductVariantId);

                if (destinationInventory is null)
                {
                    destinationInventory = new InventoryItem
                    {
                        StoreId = transfer.DestinationStoreId,
                        ProductVariantId = transferItem.ProductVariantId
                    };
                    _db.InventoryItems.Add(destinationInventory);
                }

                destinationInventory.QuantityOnHand += requested.QuantityReceived;
                destinationInventory.UpdatedAtUtc = DateTime.UtcNow;

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    StoreId = transfer.DestinationStoreId,
                    ProductVariantId = transferItem.ProductVariantId,
                    TransactionType = InventoryTransactionType.TransferIn,
                    QuantityChange = requested.QuantityReceived,
                    BalanceAfter = destinationInventory.QuantityOnHand,
                    ReferenceNumber = transfer.TransferNumber,
                    Remarks = $"Transfer received from store {transfer.SourceStoreId}",
                    CreatedByUserId = receipt.ReceivedByUserId
                });
            }

            if (requested.QuantityDamaged > 0)
            {
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    StoreId = transfer.DestinationStoreId,
                    ProductVariantId = transferItem.ProductVariantId,
                    TransactionType = InventoryTransactionType.Damage,
                    QuantityChange = 0,
                    BalanceAfter = await GetDestinationBalance(
                        transfer.DestinationStoreId,
                        transferItem.ProductVariantId),
                    ReferenceNumber = transfer.TransferNumber,
                    Remarks = $"Damaged in transit: {requested.QuantityDamaged:0.###}",
                    CreatedByUserId = receipt.ReceivedByUserId
                });
            }
        }

        var fullyProcessed = transfer.Items.All(x =>
            x.ReceivedQuantity + x.DamagedQuantity >= x.DispatchedQuantity);

        transfer.Status = fullyProcessed
            ? StockTransferStatus.FullyReceived
            : StockTransferStatus.PartiallyReceived;
        transfer.ReceivedByUserId = receipt.ReceivedByUserId;
        transfer.ReceivedAtUtc = fullyProcessed ? DateTime.UtcNow : null;
        transfer.UpdatedAtUtc = DateTime.UtcNow;

        _db.StockTransferReceipts.Add(receipt);
        await _db.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        return Ok(new
        {
            receipt.Id,
            receipt.ReceiptNumber,
            transfer.TransferNumber,
            transfer.Status,
            receipt.ReceivedAtUtc
        });
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> Audit(Guid id)
    {
        var transfer = await _db.StockTransfers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.TransferNumber,
                x.Status,
                x.TransferDateUtc,
                x.DispatchedAtUtc,
                x.ReceivedAtUtc,
                x.CreatedByUserId,
                x.DispatchedByUserId,
                x.ReceivedByUserId,
                Receipts = x.Receipts
                    .OrderBy(r => r.ReceivedAtUtc)
                    .Select(r => new
                    {
                        r.Id,
                        r.ReceiptNumber,
                        r.ReceivedAtUtc,
                        r.ReceivedByUserId,
                        r.Notes,
                        Items = r.Items.Select(i => new
                        {
                            i.ProductVariantId,
                            i.QuantityReceived,
                            i.QuantityDamaged
                        })
                    })
            })
            .SingleOrDefaultAsync();

        return transfer is null ? NotFound() : Ok(transfer);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("User identifier is missing.");
    }

    private async Task<decimal> GetDestinationBalance(
        Guid storeId,
        Guid productVariantId)
    {
        return await _db.InventoryItems
            .Where(x => x.StoreId == storeId &&
                        x.ProductVariantId == productVariantId)
            .Select(x => (decimal?)x.QuantityOnHand)
            .SingleOrDefaultAsync() ?? 0m;
    }

    private async Task<string> GenerateTransferNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var count = await _db.StockTransfers.CountAsync(x =>
            x.TransferDateUtc.Date == DateTime.UtcNow.Date);
        return $"TR-{datePart}-{count + 1:00000}";
    }

    private async Task<string> GenerateReceiptNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var count = await _db.StockTransferReceipts.CountAsync(x =>
            x.ReceivedAtUtc.Date == DateTime.UtcNow.Date);
        return $"TRR-{datePart}-{count + 1:00000}";
    }
}
