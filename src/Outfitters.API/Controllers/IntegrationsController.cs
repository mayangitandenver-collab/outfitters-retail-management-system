using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Integrations;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure.Integrations;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize]
[Route("api/integrations")]
public sealed class IntegrationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IBarcodeService _barcodeService;
    private readonly IReceiptFormatter _receiptFormatter;

    public IntegrationsController(
        ApplicationDbContext db,
        IBarcodeService barcodeService,
        IReceiptFormatter receiptFormatter)
    {
        _db = db;
        _barcodeService = barcodeService;
        _receiptFormatter = receiptFormatter;
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,Cashier")]
    [HttpPost("barcodes")]
    public async Task<IActionResult> CreateBarcode(
        CreateBarcodeAliasRequest request)
    {
        var barcode = _barcodeService.Normalize(request.Barcode);

        if (!_barcodeService.IsValid(barcode, request.BarcodeType))
        {
            return BadRequest("Barcode format is invalid.");
        }

        if (!await _db.ProductVariants.AnyAsync(x =>
            x.Id == request.ProductVariantId))
        {
            return NotFound("Product variant was not found.");
        }

        if (await _db.BarcodeAliases.AnyAsync(x => x.Barcode == barcode))
        {
            return Conflict("Barcode already exists.");
        }

        if (request.IsPrimary)
        {
            var currentPrimary = await _db.BarcodeAliases
                .Where(x =>
                    x.ProductVariantId == request.ProductVariantId &&
                    x.IsPrimary)
                .ToListAsync();

            foreach (var item in currentPrimary)
            {
                item.IsPrimary = false;
            }
        }

        var alias = new BarcodeAlias
        {
            ProductVariantId = request.ProductVariantId,
            Barcode = barcode,
            BarcodeType = request.BarcodeType.Trim().ToUpperInvariant(),
            IsPrimary = request.IsPrimary
        };

        _db.BarcodeAliases.Add(alias);
        await _db.SaveChangesAsync();

        return Ok(alias);
    }

    [HttpGet("barcodes/{barcode}")]
    public async Task<IActionResult> LookupBarcode(string barcode)
    {
        var normalized = _barcodeService.Normalize(barcode);

        var result = await _db.BarcodeAliases
            .AsNoTracking()
            .Where(x => x.Barcode == normalized && x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.Barcode,
                x.BarcodeType,
                x.ProductVariantId,
                ProductName = x.ProductVariant.Product.Name,
                x.IsPrimary
            })
            .SingleOrDefaultAsync();

        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager,Cashier")]
    [HttpPost("receipts/queue")]
    public async Task<IActionResult> QueueReceipt(
        QueueReceiptPrintRequest request)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.SaleId);

        if (sale is null)
        {
            return NotFound("Sale was not found.");
        }

        var payload = await _receiptFormatter.FormatSaleAsync(request.SaleId);

        var printJob = new ReceiptPrintJob
        {
            SaleId = sale.Id,
            StoreId = sale.StoreId,
            PrinterName = request.PrinterName.Trim(),
            CopyCount = Math.Clamp(request.CopyCount, 1, 5),
            ReceiptPayload = payload
        };

        _db.ReceiptPrintJobs.Add(printJob);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            printJob.Id,
            printJob.Status,
            printJob.PrinterName,
            printJob.CopyCount
        });
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpPost("notifications")]
    public async Task<IActionResult> QueueNotification(
        QueueNotificationRequest request)
    {
        var message = new NotificationMessage
        {
            Channel = request.Channel,
            Recipient = request.Recipient.Trim(),
            Subject = request.Subject.Trim(),
            Body = request.Body,
            TemplateCode = request.TemplateCode?.Trim(),
            ReferenceType = request.ReferenceType?.Trim(),
            ReferenceId = request.ReferenceId?.Trim(),
            ScheduledAtUtc = request.ScheduledAtUtc
        };

        _db.NotificationMessages.Add(message);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message.Id,
            message.Status,
            message.ScheduledAtUtc
        });
    }

    [Authorize(Roles = "SuperAdministrator,Administrator")]
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSetting(
        UpdateIntegrationSettingRequest request)
    {
        var provider = request.ProviderCode.Trim().ToUpperInvariant();
        var key = request.SettingKey.Trim();

        var setting = await _db.IntegrationSettings.SingleOrDefaultAsync(x =>
            x.ProviderCode == provider &&
            x.SettingKey == key);

        if (setting is null)
        {
            setting = new IntegrationSetting
            {
                ProviderCode = provider,
                SettingKey = key
            };
            _db.IntegrationSettings.Add(setting);
        }

        setting.SettingValue = request.SettingValue;
        setting.IsSecret = request.IsSecret;
        setting.IsEnabled = request.IsEnabled;
        setting.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "SuperAdministrator,Administrator,StoreManager")]
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var result = new
        {
            Database = "Healthy",
            PendingNotifications = await _db.NotificationMessages.CountAsync(x =>
                x.Status == Domain.Enums.NotificationStatus.Pending),
            PendingPrintJobs = await _db.ReceiptPrintJobs.CountAsync(x =>
                x.Status == Domain.Enums.ReceiptPrintStatus.Pending),
            ActiveBarcodeAliases = await _db.BarcodeAliases.CountAsync(x =>
                x.IsActive),
            EnabledProviders = await _db.IntegrationSettings
                .Where(x => x.IsEnabled)
                .Select(x => x.ProviderCode)
                .Distinct()
                .ToListAsync()
        };

        return Ok(result);
    }
}
