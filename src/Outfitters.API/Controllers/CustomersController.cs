using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Customers;
using Outfitters.Domain.Entities;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CustomersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);
        var query = _db.Customers
            .AsNoTracking()
            .Include(x => x.CustomerTier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.CustomerNumber.ToLower().Contains(term) ||
                x.FirstName.ToLower().Contains(term) ||
                x.LastName.ToLower().Contains(term) ||
                (x.Email != null && x.Email.ToLower().Contains(term)) ||
                (x.Phone != null && x.Phone.Contains(term)));
        }

        var result = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.CustomerNumber,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Phone,
                Tier = x.CustomerTier == null ? null : x.CustomerTier.Name,
                x.LoyaltyPointsBalance,
                x.StoreCreditBalance,
                x.LifetimeSpend,
                x.LastPurchaseAtUtc,
                x.IsActive
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.CustomerNumber,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Phone,
                x.BirthDate,
                x.Address,
                x.CustomerTierId,
                Tier = x.CustomerTier == null ? null : x.CustomerTier.Name,
                x.LoyaltyPointsBalance,
                x.StoreCreditBalance,
                x.LifetimeSpend,
                x.LastPurchaseAtUtc,
                x.AcceptsEmailMarketing,
                x.AcceptsSmsMarketing,
                x.IsActive
            })
            .SingleOrDefaultAsync();

        return customer is null ? NotFound() : Ok(customer);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,Cashier")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("First and last name are required.");
        }

        var customer = new Customer
        {
            CustomerNumber = await GenerateCustomerNumber(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            BirthDate = request.BirthDate,
            Address = request.Address?.Trim(),
            AcceptsEmailMarketing = request.AcceptsEmailMarketing,
            AcceptsSmsMarketing = request.AcceptsSmsMarketing
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            customer.Id,
            customer.CustomerNumber
        });
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _db.Customers.SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = request.Email?.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.BirthDate = request.BirthDate;
        customer.Address = request.Address?.Trim();
        customer.CustomerTierId = request.CustomerTierId;
        customer.AcceptsEmailMarketing = request.AcceptsEmailMarketing;
        customer.AcceptsSmsMarketing = request.AcceptsSmsMarketing;
        customer.IsActive = request.IsActive;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpPost("{id:guid}/loyalty/adjust")]
    public async Task<IActionResult> AdjustLoyalty(
        Guid id,
        AdjustLoyaltyPointsRequest request)
    {
        var customer = await _db.Customers.SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        var newBalance = customer.LoyaltyPointsBalance + request.PointsChange;
        if (newBalance < 0)
        {
            return BadRequest("Loyalty balance cannot be negative.");
        }

        customer.LoyaltyPointsBalance = newBalance;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        _db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            CustomerId = customer.Id,
            Type = LoyaltyTransactionType.Adjustment,
            PointsChange = request.PointsChange,
            BalanceAfter = newBalance,
            ReferenceNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Notes = request.Notes?.Trim()
        });

        await _db.SaveChangesAsync();

        return Ok(new
        {
            customer.Id,
            customer.LoyaltyPointsBalance
        });
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpPost("{id:guid}/store-credit/adjust")]
    public async Task<IActionResult> AdjustStoreCredit(
        Guid id,
        AdjustStoreCreditRequest request)
    {
        var customer = await _db.Customers.SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        var newBalance = customer.StoreCreditBalance + request.AmountChange;
        if (newBalance < 0)
        {
            return BadRequest("Store-credit balance cannot be negative.");
        }

        customer.StoreCreditBalance = newBalance;
        customer.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            customer.Id,
            customer.StoreCreditBalance,
            request.Notes
        });
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpPost("{id:guid}/vouchers")]
    public async Task<IActionResult> CreateVoucher(
        Guid id,
        CreateVoucherRequest request)
    {
        if (!await _db.Customers.AnyAsync(x => x.Id == id))
        {
            return NotFound();
        }

        if (request.DiscountAmount < 0 ||
            request.DiscountPercent < 0 ||
            request.MinimumSpend < 0 ||
            request.ValidUntilUtc <= DateTime.UtcNow)
        {
            return BadRequest("Voucher values are invalid.");
        }

        var voucher = new CustomerVoucher
        {
            CustomerId = id,
            Code = $"VCH-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
            DiscountAmount = request.DiscountAmount,
            DiscountPercent = request.DiscountPercent,
            MinimumSpend = request.MinimumSpend,
            ValidUntilUtc = request.ValidUntilUtc
        };

        _db.CustomerVouchers.Add(voucher);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            voucher.Id,
            voucher.Code,
            voucher.ValidUntilUtc,
            voucher.Status
        });
    }

    [HttpGet("{id:guid}/purchase-history")]
    public async Task<IActionResult> PurchaseHistory(Guid id)
    {
        if (!await _db.Customers.AnyAsync(x => x.Id == id))
        {
            return NotFound();
        }

        var result = await _db.Sales
            .AsNoTracking()
            .Where(x => x.CustomerId == id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.ReceiptNumber,
                x.StoreId,
                x.GrandTotal,
                x.Status,
                x.CreatedAtUtc,
                Items = x.Items.Select(i => new
                {
                    i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                })
            })
            .ToListAsync();

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,Cashier")]
    [HttpPost("{id:guid}/favorites")]
    public async Task<IActionResult> AddFavorite(
        Guid id,
        AddFavoriteProductRequest request)
    {
        if (!await _db.Customers.AnyAsync(x => x.Id == id) ||
            !await _db.Products.AnyAsync(x => x.Id == request.ProductId))
        {
            return NotFound();
        }

        if (await _db.CustomerFavoriteProducts.AnyAsync(x =>
            x.CustomerId == id && x.ProductId == request.ProductId))
        {
            return Conflict("Product is already a favorite.");
        }

        _db.CustomerFavoriteProducts.Add(new CustomerFavoriteProduct
        {
            CustomerId = id,
            ProductId = request.ProductId
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> GenerateCustomerNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var count = await _db.Customers.CountAsync(x =>
            x.CreatedAtUtc.Date == DateTime.UtcNow.Date);
        return $"CUS-{datePart}-{count + 1:00000}";
    }
}
