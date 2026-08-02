using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Employees;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
[Route("api/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<ApplicationRole> _roles;

    public EmployeesController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        RoleManager<ApplicationRole> roles)
    {
        _db = db;
        _users = users;
        _roles = roles;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeId)
    {
        var query = _db.EmployeeProfiles.AsNoTracking()
            .Include(x => x.User).Include(x => x.PrimaryStore).AsQueryable();
        if (storeId.HasValue)
            query = query.Where(x => x.PrimaryStoreId == storeId.Value);

        var employees = await query.OrderBy(x => x.User.LastName).ToListAsync();
        var result = new List<object>();
        foreach (var employee in employees)
        {
            result.Add(new
            {
                employee.Id,
                employee.EmployeeNumber,
                employee.User.FirstName,
                employee.User.LastName,
                employee.User.Email,
                employee.JobTitle,
                employee.Status,
                employee.PrimaryStoreId,
                PrimaryStore = employee.PrimaryStore == null ? null : employee.PrimaryStore.Name,
                Roles = await _users.GetRolesAsync(employee.User)
            });
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequest request)
    {
        if (await _users.FindByEmailAsync(request.Email.Trim()) is not null)
            return Conflict("Email already exists.");

        foreach (var role in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            if (!await _roles.RoleExistsAsync(role))
                return BadRequest($"Role '{role}' does not exist.");

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            StoreId = request.PrimaryStoreId,
            EmailConfirmed = true
        };

        var created = await _users.CreateAsync(user, request.TemporaryPassword);
        if (!created.Succeeded) return BadRequest(created.Errors);

        if (request.Roles.Count > 0)
        {
            var roleResult = await _users.AddToRolesAsync(user, request.Roles.Distinct());
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);
        }

        var employee = new EmployeeProfile
        {
            EmployeeNumber = await GenerateEmployeeNumber(),
            UserId = user.Id,
            PrimaryStoreId = request.PrimaryStoreId,
            JobTitle = request.JobTitle.Trim(),
            HireDate = request.HireDate,
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone?.Trim()
        };

        if (request.PrimaryStoreId.HasValue)
            employee.StoreAssignments.Add(new EmployeeStoreAssignment
            {
                StoreId = request.PrimaryStoreId.Value,
                IsPrimary = true
            });

        _db.EmployeeProfiles.Add(employee);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "CreateEmployee",
            EntityName = nameof(EmployeeProfile),
            EntityId = employee.Id.ToString(),
            DetailsJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                employee.EmployeeNumber,
                request.Roles,
                request.PrimaryStoreId
            })
        });
        await _db.SaveChangesAsync();

        return Ok(new { employee.Id, employee.EmployeeNumber, employee.UserId });
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRoles(Guid id, AssignRolesRequest request)
    {
        var employee = await _db.EmployeeProfiles.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (employee is null) return NotFound();

        foreach (var role in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            if (!await _roles.RoleExistsAsync(role))
                return BadRequest($"Role '{role}' does not exist.");

        var current = await _users.GetRolesAsync(employee.User);
        var removed = await _users.RemoveFromRolesAsync(employee.User, current);
        if (!removed.Succeeded) return BadRequest(removed.Errors);

        var added = await _users.AddToRolesAsync(employee.User, request.Roles.Distinct());
        if (!added.Succeeded) return BadRequest(added.Errors);

        return NoContent();
    }

    [HttpPost("{id:guid}/attendance")]
    public async Task<IActionResult> Attendance(Guid id, RecordAttendanceRequest request)
    {
        if (!await _db.EmployeeProfiles.AnyAsync(x => x.Id == id)) return NotFound();

        var record = await _db.EmployeeAttendanceRecords.SingleOrDefaultAsync(x =>
            x.EmployeeProfileId == id && x.WorkDate == request.WorkDate);
        if (record is null)
        {
            record = new EmployeeAttendance { EmployeeProfileId = id, WorkDate = request.WorkDate };
            _db.EmployeeAttendanceRecords.Add(record);
        }
        record.ClockInAtUtc = request.ClockInAtUtc;
        record.ClockOutAtUtc = request.ClockOutAtUtc;
        record.Status = request.Status;
        record.Notes = request.Notes?.Trim();
        record.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(record);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator")]
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetEmployeePasswordRequest request)
    {
        var employee = await _db.EmployeeProfiles.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (employee is null) return NotFound();

        var token = await _users.GeneratePasswordResetTokenAsync(employee.User);
        var result = await _users.ResetPasswordAsync(employee.User, token, request.NewPassword);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }

    private async Task<string> GenerateEmployeeNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var count = await _db.EmployeeProfiles.CountAsync(x => x.CreatedAtUtc.Date == DateTime.UtcNow.Date);
        return $"EMP-{datePart}-{count + 1:00000}";
    }
}
