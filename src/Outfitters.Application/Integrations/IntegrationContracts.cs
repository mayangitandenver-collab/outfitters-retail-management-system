using Outfitters.Domain.Enums;

namespace Outfitters.Application.Integrations;

public sealed record QueueNotificationRequest(
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    string? TemplateCode,
    string? ReferenceType,
    string? ReferenceId,
    DateTime? ScheduledAtUtc);

public sealed record CreateBarcodeAliasRequest(
    Guid ProductVariantId,
    string Barcode,
    string BarcodeType,
    bool IsPrimary);

public sealed record QueueReceiptPrintRequest(
    Guid SaleId,
    string PrinterName,
    int CopyCount);

public sealed record UpdateIntegrationSettingRequest(
    string ProviderCode,
    string SettingKey,
    string SettingValue,
    bool IsSecret,
    bool IsEnabled);
