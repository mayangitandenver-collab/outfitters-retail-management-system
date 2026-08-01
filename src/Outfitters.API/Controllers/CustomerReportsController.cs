using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
[Route("api/customer-reports")]
public sealed class CustomerReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CustomerReportsController(ApplicationDbContext db) => _db = db;

    [HttpGet("top-customers")]
    public async Task<IActionResult> TopCustomers([FromQuery] int limit = 25)
    {
        limit = Math.Clamp(limit, 1, 100);

        var result = await _db.Customers
            .AsNoTracking()
            .OrderByDescending(x => x.LifetimeSpend)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.CustomerNumber,
                CustomerName = x.FirstName + " " + x.LastName,
                Tier = x.CustomerTier == null ? null : x.CustomerTier.Name,
                x.LifetimeSpend,
                x.LoyaltyPointsBalance,
                x.StoreCreditBalance,
                x.LastPurchaseAtUtc,
                TransactionCount = _db.Sales.Count(s =>
                    s.CustomerId == x.Id &&
                    s.Status != SaleStatus.Voided)
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("segments")]
    public async Task<IActionResult> Segments()
    {
        var now = DateTime.UtcNow;

        var result = new
        {
            NewCustomers = await _db.Customers.CountAsync(x =>
                x.CreatedAtUtc >= now.AddDays(-30)),
            ActiveCustomers = await _db.Customers.CountAsync(x =>
                x.LastPurchaseAtUtc >= now.AddDays(-90)),
            AtRiskCustomers = await _db.Customers.CountAsync(x =>
                x.LastPurchaseAtUtc < now.AddDays(-90) &&
                x.LastPurchaseAtUtc >= now.AddDays(-180)),
            DormantCustomers = await _db.Customers.CountAsync(x =>
                x.LastPurchaseAtUtc < now.AddDays(-180)),
            BirthdayCustomersThisMonth = await _db.Customers.CountAsync(x =>
                x.BirthDate.HasValue &&
                x.BirthDate.Value.Month == now.Month),
            EmailSubscribers = await _db.Customers.CountAsync(x =>
                x.AcceptsEmailMarketing),
            SmsSubscribers = await _db.Customers.CountAsync(x =>
                x.AcceptsSmsMarketing)
        };

        return Ok(result);
    }

    [HttpGet("lifetime-value")]
    public async Task<IActionResult> LifetimeValue([FromQuery] int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);

        var result = await _db.Customers
            .AsNoTracking()
            .OrderByDescending(x => x.LifetimeSpend)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.CustomerNumber,
                CustomerName = x.FirstName + " " + x.LastName,
                x.LifetimeSpend,
                Purchases = _db.Sales.Count(s =>
                    s.CustomerId == x.Id &&
                    s.Status != SaleStatus.Voided),
                AverageOrderValue = _db.Sales
                    .Where(s =>
                        s.CustomerId == x.Id &&
                        s.Status != SaleStatus.Voided)
                    .Average(s => (decimal?)s.GrandTotal) ?? 0m,
                FirstPurchaseAtUtc = _db.Sales
                    .Where(s =>
                        s.CustomerId == x.Id &&
                        s.Status != SaleStatus.Voided)
                    .Min(s => (DateTime?)s.CreatedAtUtc),
                x.LastPurchaseAtUtc
            })
            .ToListAsync();

        return Ok(result);
    }
}
