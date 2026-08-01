using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Sales;
using Outfitters.Domain.Entities;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,Cashier")]
[Route("api/sales")]
public sealed class SalesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SalesController(ApplicationDbContext db) => _db = db;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one sale item is required.");
        }

        if (request.Payments.Count == 0)
        {
            return BadRequest("At least one payment is required.");
        }

        var cashSession = await _db.CashSessions.SingleOrDefaultAsync(x =>
            x.Id == request.CashSessionId &&
            x.StoreId == request.StoreId &&
            x.Status == CashSessionStatus.Open);

        if (cashSession is null)
        {
            return BadRequest("An open cash session was not found.");
        }

        var cashierUserId = GetUserId();
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var sale = new Sale
        {
            StoreId = request.StoreId,
            CashSessionId = request.CashSessionId,
            CashierUserId = cashierUserId,
            ReceiptNumber = await GenerateReceiptNumber(request.StoreId),
            Notes = request.Notes?.Trim()
        };

        foreach (var requestedItem in request.Items)
        {
            if (requestedItem.Quantity <= 0)
            {
                return BadRequest("Item quantity must be greater than zero.");
            }

            if (requestedItem.UnitPrice < 0 ||
                requestedItem.DiscountAmount < 0 ||
                requestedItem.TaxAmount < 0)
            {
                return BadRequest("Price, discount, and tax values cannot be negative.");
            }

            var inventory = await _db.InventoryItems
                .SingleOrDefaultAsync(x =>
                    x.StoreId == request.StoreId &&
                    x.ProductVariantId == requestedItem.ProductVariantId);

            if (inventory is null ||
                inventory.QuantityOnHand - inventory.ReservedQuantity < requestedItem.Quantity)
            {
                return BadRequest(
                    $"Insufficient stock for variant {requestedItem.ProductVariantId}.");
            }

            var gross = requestedItem.Quantity * requestedItem.UnitPrice;
            var lineTotal = gross - requestedItem.DiscountAmount + requestedItem.TaxAmount;

            if (lineTotal < 0)
            {
                return BadRequest("Line total cannot be negative.");
            }

            sale.Items.Add(new SaleItem
            {
                ProductVariantId = requestedItem.ProductVariantId,
                Quantity = requestedItem.Quantity,
                UnitPrice = requestedItem.UnitPrice,
                DiscountAmount = requestedItem.DiscountAmount,
                TaxAmount = requestedItem.TaxAmount,
                LineTotal = lineTotal
            });

            inventory.QuantityOnHand -= requestedItem.Quantity;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                StoreId = request.StoreId,
                ProductVariantId = requestedItem.ProductVariantId,
                TransactionType = InventoryTransactionType.Sale,
                QuantityChange = -requestedItem.Quantity,
                BalanceAfter = inventory.QuantityOnHand,
                ReferenceNumber = sale.ReceiptNumber,
                Remarks = "POS sale",
                CreatedByUserId = cashierUserId
            });
        }

        sale.Subtotal = sale.Items.Sum(x => x.Quantity * x.UnitPrice);
        sale.DiscountTotal = sale.Items.Sum(x => x.DiscountAmount);
        sale.TaxTotal = sale.Items.Sum(x => x.TaxAmount);
        sale.GrandTotal = sale.Items.Sum(x => x.LineTotal);

        foreach (var requestedPayment in request.Payments)
        {
            if (requestedPayment.Amount <= 0)
            {
                return BadRequest("Payment amount must be greater than zero.");
            }

            sale.Payments.Add(new SalePayment
            {
                Method = requestedPayment.Method,
                Amount = requestedPayment.Amount,
                ReferenceNumber = requestedPayment.ReferenceNumber?.Trim()
            });
        }

        sale.AmountPaid = sale.Payments.Sum(x => x.Amount);
        if (sale.AmountPaid < sale.GrandTotal)
        {
            return BadRequest("Payment total is less than the sale total.");
        }

        sale.ChangeDue = sale.AmountPaid - sale.GrandTotal;

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            sale.Id,
            sale.ReceiptNumber,
            sale.Subtotal,
            sale.DiscountTotal,
            sale.TaxTotal,
            sale.GrandTotal,
            sale.AmountPaid,
            sale.ChangeDue,
            sale.CreatedAtUtc
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ReceiptNumber,
                x.StoreId,
                x.CashierUserId,
                x.Subtotal,
                x.DiscountTotal,
                x.TaxTotal,
                x.GrandTotal,
                x.AmountPaid,
                x.ChangeDue,
                x.Status,
                x.CreatedAtUtc,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ProductVariantId,
                    i.Quantity,
                    i.UnitPrice,
                    i.DiscountAmount,
                    i.TaxAmount,
                    i.LineTotal,
                    i.ReturnedQuantity
                }),
                Payments = x.Payments.Select(p => new
                {
                    p.Method,
                    p.Amount,
                    p.ReferenceNumber
                })
            })
            .SingleOrDefaultAsync();

        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpGet("{id:guid}/receipt")]
    public async Task<IActionResult> Receipt(Guid id)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Include(x => x.Store)
            .Include(x => x.CashierUser)
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
                .ThenInclude(x => x.Product)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (sale is null)
        {
            return NotFound();
        }

        var builder = new StringBuilder();
        builder.AppendLine("OUTFITTERS APPAREL STORE");
        builder.AppendLine(sale.Store.Name);
        builder.AppendLine($"Receipt: {sale.ReceiptNumber}");
        builder.AppendLine($"Date: {sale.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC");
        builder.AppendLine($"Cashier: {sale.CashierUser.UserName}");
        builder.AppendLine(new string('-', 32));

        foreach (var item in sale.Items)
        {
            builder.AppendLine(item.ProductVariant.Product.Name);
            builder.AppendLine(
                $"{item.Quantity.ToString("0.###", CultureInfo.InvariantCulture)} x " +
                $"{item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)} " +
                $"= {item.LineTotal.ToString("0.00", CultureInfo.InvariantCulture)}");
        }

        builder.AppendLine(new string('-', 32));
        builder.AppendLine($"Subtotal: {sale.Subtotal:0.00}");
        builder.AppendLine($"Discount: {sale.DiscountTotal:0.00}");
        builder.AppendLine($"Tax: {sale.TaxTotal:0.00}");
        builder.AppendLine($"TOTAL: {sale.GrandTotal:0.00}");
        builder.AppendLine($"Paid: {sale.AmountPaid:0.00}");
        builder.AppendLine($"Change: {sale.ChangeDue:0.00}");
        builder.AppendLine("Thank you for shopping!");

        return Content(builder.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> DailySummary(
        [FromQuery] Guid storeId,
        [FromQuery] DateOnly? date)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = selectedDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var sales = _db.Sales
            .AsNoTracking()
            .Where(x =>
                x.StoreId == storeId &&
                x.CreatedAtUtc >= start &&
                x.CreatedAtUtc < end &&
                x.Status != SaleStatus.Voided);

        var result = new
        {
            Date = selectedDate,
            Transactions = await sales.CountAsync(),
            GrossSales = await sales.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m,
            Discounts = await sales.SumAsync(x => (decimal?)x.DiscountTotal) ?? 0m,
            Taxes = await sales.SumAsync(x => (decimal?)x.TaxTotal) ?? 0m,
            UnitsSold = await sales.SelectMany(x => x.Items)
                .SumAsync(x => (decimal?)x.Quantity) ?? 0m
        };

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("User identifier is missing.");
    }

    private async Task<string> GenerateReceiptNumber(Guid storeId)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var countToday = await _db.Sales.CountAsync(x =>
            x.StoreId == storeId &&
            x.CreatedAtUtc.Date == DateTime.UtcNow.Date);

        return $"OR-{datePart}-{countToday + 1:00000}";
    }
}
