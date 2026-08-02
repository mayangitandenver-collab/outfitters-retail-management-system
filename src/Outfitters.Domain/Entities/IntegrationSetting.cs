using Outfitters.Domain.Common;

namespace Outfitters.Domain.Entities;

public sealed class IntegrationSetting : BaseEntity
{
    public string ProviderCode { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public bool IsEnabled { get; set; } = true;
}
