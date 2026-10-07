using Kt20Chat.Data;
using Kt20Chat.Hubs;
using Kt20Chat.Models;
using Kt20Chat.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
{
    DataSource = Path.Combine(dataDirectory, "chat.db")
};
var settings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var keyPath = Path.Combine(dataDirectory, "jwt.key");
if (!File.Exists(keyPath)) File.WriteAllBytes(keyPath, System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
var key = File.ReadAllBytes(keyPath);
if (key.Length < 32) throw new InvalidOperationException("Ключ JWT должен содержать не меньше 32 байт");
settings.SigningKey = new SymmetricSecurityKey(key);
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<TokenSessionStore>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection.ConnectionString));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = settings.SigningKey,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "name"
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/chathub"))
                    context.Token = token;
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var sessions = context.HttpContext.RequestServices.GetRequiredService<TokenSessionStore>();
                if (sessions.IsRevoked(context.Principal?.FindFirst("jti")?.Value))
                    context.Fail("Токен больше не действует");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chathub", options => options.CloseOnAuthenticationExpiration = true).RequireAuthorization();
app.Run();
