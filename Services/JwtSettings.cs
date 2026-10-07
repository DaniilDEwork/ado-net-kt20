using Microsoft.IdentityModel.Tokens;

namespace Kt20Chat.Services;

public class JwtSettings
{
    public string Issuer { get; set; } = "Kt20Chat";
    public string Audience { get; set; } = "Kt20ChatUsers";
    public int LifetimeMinutes { get; set; } = 120;
    public SymmetricSecurityKey SigningKey { get; set; } = null!;
}
