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
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,Cashier")]
[Route("api/cash-sessions")]
public sealed class CashSessionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CashSessionsController(ApplicationDbContext db) => _db = db;

    [HttpPost("open")]
    public async Task<IActionResult> Open(OpenCashSessionRequest request)
    {
        if (request.OpeningCash < 0)
        {
            return BadRequest("Opening cash cannot be negative.");
        }

        if (!await _db.Stores.AnyAsync(x => x.Id == request.StoreId && x.IsActive))
        {
            return BadRequest("Store was not found.");
        }

        var userId = GetUserId();
        var alreadyOpen = await _db.CashSessions.AnyAsync(x =>
            x.StoreId == request.StoreId &&
            x.OpenedByUserId == userId &&
            x.Status == CashSessionStatus.Open);

        if (alreadyOpen)
        {
            return Conflict("This user already has an open cash session for the store.");
        }

        var session = new CashSession
        {
            StoreId = request.StoreId,
            OpenedByUserId = userId,
            OpeningCash = request.OpeningCash
        };

        _db.CashSessions.Add(session);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            session.Id,
            session.StoreId,
            session.OpenedAtUtc,
            session.OpeningCash,
            session.Status
        });
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CloseCashSessionRequest request)
    {
        var session = await _db.CashSessions
            .SingleOrDefaultAsync(x => x.Id == id);

        if (session is null)
        {
            return NotFound();
        }

        if (session.Status != CashSessionStatus.Open)
        {
            return BadRequest("Cash session is already closed.");
        }

        var cashSales = await _db.SalePayments
            .Where(x =>
                x.Sale.CashSessionId == id &&
                x.Sale.Status != SaleStatus.Voided &&
                x.Method == PaymentMethod.Cash)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        var expectedCash = session.OpeningCash + cashSales;
        session.ClosingCash = request.ClosingCash;
        session.ExpectedCash = expectedCash;
        session.CashVariance = request.ClosingCash - expectedCash;
        session.ClosedAtUtc = DateTime.UtcNow;
        session.ClosedByUserId = GetUserId();
        session.Status = CashSessionStatus.Closed;
        session.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            session.Id,
            session.OpeningCash,
            session.ExpectedCash,
            session.ClosingCash,
            session.CashVariance,
            session.ClosedAtUtc
        });
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen([FromQuery] Guid storeId)
    {
        var userId = GetUserId();
        var sessions = await _db.CashSessions
            .AsNoTracking()
            .Where(x =>
                x.StoreId == storeId &&
                x.OpenedByUserId == userId &&
                x.Status == CashSessionStatus.Open)
            .OrderByDescending(x => x.OpenedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.StoreId,
                x.OpenedAtUtc,
                x.OpeningCash,
                x.Status
            })
            .ToListAsync();

        return Ok(sessions);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("User identifier is missing.");
    }
}
