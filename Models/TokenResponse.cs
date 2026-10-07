namespace Kt20Chat.Models;

public record TokenResponse(string AccessToken, string UserName, DateTimeOffset ExpiresAt);
