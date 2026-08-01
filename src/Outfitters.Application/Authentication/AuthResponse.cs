namespace Outfitters.Application.Authentication;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Username,
    IReadOnlyCollection<string> Roles);
