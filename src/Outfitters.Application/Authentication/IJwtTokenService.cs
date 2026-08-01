using Outfitters.Domain.Entities;

namespace Outfitters.Application.Authentication;

public interface IJwtTokenService
{
    AuthResponse CreateToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
