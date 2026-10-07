using Kt20Chat.Data;
using Kt20Chat.Models;
using Kt20Chat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kt20Chat.Controllers;

[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/auth")]
public class AuthController(AppDbContext database, IPasswordHasher<AppUser> passwordHasher,
    JwtTokenService tokens, TokenSessionStore sessions) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var normalizedName = model.UserName.ToUpperInvariant();
        if (await database.Users.AnyAsync(user => user.NormalizedUserName == normalizedName))
            return Conflict(new { message = "Пользователь с таким именем уже существует" });
        var user = new AppUser { UserName = model.UserName, NormalizedUserName = normalizedName };
        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
        database.Users.Add(user);
        try
        {
            await database.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Не удалось зарегистрироваться, попробуйте другое имя" });
        }
        return StatusCode(StatusCodes.Status201Created, tokens.Create(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var normalizedName = model.UserName.ToUpperInvariant();
        var user = await database.Users.SingleOrDefaultAsync(user => user.NormalizedUserName == normalizedName);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
        return Ok(tokens.Create(user));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { userName = User.Identity!.Name, id = User.FindFirst("sub")!.Value });

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var expiry = long.Parse(User.FindFirst("exp")!.Value);
        sessions.Revoke(User.FindFirst("jti")!.Value, DateTimeOffset.FromUnixTimeSeconds(expiry));
        return Ok(new { message = "Вы вышли из аккаунта" });
    }
}
