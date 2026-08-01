using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Catalog;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/brands")]
public sealed class BrandsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BrandsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Brands
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Description, x.IsActive })
            .ToListAsync();

        return Ok(items);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBrandRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Brand name is required.");
        }

        if (await _db.Brands.AnyAsync(x => x.Name == name))
        {
            return Conflict("A brand with this name already exists.");
        }

        var brand = new Brand
        {
            Name = name,
            Description = request.Description?.Trim()
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();

        return Ok(brand);
    }
}
