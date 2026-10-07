using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Kt20Chat.Models;
using Microsoft.IdentityModel.Tokens;

namespace Kt20Chat.Services;

public class JwtTokenService(JwtSettings settings)
{
    public TokenResponse Create(AppUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(settings.LifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("name", user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            notBefore: now.UtcDateTime, expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(settings.SigningKey, SecurityAlgorithms.HmacSha256));
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), user.UserName, expiresAt);
    }
}
