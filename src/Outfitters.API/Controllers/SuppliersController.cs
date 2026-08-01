using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Purchasing;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/suppliers")]
public sealed class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SuppliersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _db.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Code.ToLower().Contains(term) ||
                x.Name.ToLower().Contains(term));
        }

        return Ok(await query.OrderBy(x => x.Name).ToListAsync());
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,InventoryClerk")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateSupplierRequest request)
    {
        var code = request.Code.Trim();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Supplier code and name are required.");
        }

        if (await _db.Suppliers.AnyAsync(x => x.Code == code))
        {
            return Conflict("Supplier code already exists.");
        }

        var supplier = new Supplier
        {
            Code = code,
            Name = name,
            ContactPerson = request.ContactPerson?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            TaxIdentificationNumber = request.TaxIdentificationNumber?.Trim(),
            PaymentTermsDays = request.PaymentTermsDays
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return Ok(supplier);
    }
}
