using Microsoft.AspNetCore.Identity;

namespace Outfitters.Domain.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
