using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/system")]
public sealed class ReleaseReadinessController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public ReleaseReadinessController(
        ApplicationDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [HttpGet("health/live")]
    public IActionResult Liveness()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "Outfitters Retail Management System",
            Environment = _environment.EnvironmentName,
            UtcTime = DateTime.UtcNow
        });
    }

    [HttpGet("health/ready")]
    public async Task<IActionResult> Readiness(
        CancellationToken cancellationToken)
    {
        try
        {
            var databaseReady = await _db.Database
                .CanConnectAsync(cancellationToken);

            if (!databaseReady)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        Status = "Unhealthy",
                        Database = "Unavailable",
                        UtcTime = DateTime.UtcNow
                    });
            }

            return Ok(new
            {
                Status = "Ready",
                Database = "Connected",
                UtcTime = DateTime.UtcNow
            });
        }
        catch (Exception exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    Status = "Unhealthy",
                    Database = "Error",
                    Error = exception.Message,
                    UtcTime = DateTime.UtcNow
                });
        }
    }
}
