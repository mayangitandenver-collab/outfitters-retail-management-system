using Microsoft.AspNetCore.Identity;

namespace Outfitters.Domain.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public Store? Store { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    public DateTime? LastLoginAtUtc { get; set; }
}
