using Kt20Chat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Kt20Chat.Hubs;

[Authorize]
public class ChatHub(TokenSessionStore sessions) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (!sessions.AddConnection(Context.User?.FindFirst("jti")?.Value, Context.ConnectionId, Context.Abort))
        {
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        sessions.RemoveConnection(Context.User?.FindFirst("jti")?.Value, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string? message)
    {
        var expiry = Context.User?.FindFirst("exp")?.Value;
        if (sessions.IsRevoked(Context.User?.FindFirst("jti")?.Value)
            || !long.TryParse(expiry, out var seconds) || seconds <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            Context.Abort();
            throw new HubException("Войдите в аккаунт снова");
        }
        message = message?.Trim();
        if (string.IsNullOrEmpty(message)) throw new HubException("Введите сообщение");
        if (message.Length > 500) throw new HubException("Сообщение не должно быть длиннее 500 символов");
        var userName = Context.User?.Identity?.Name;
        if (string.IsNullOrEmpty(userName)) throw new HubException("Не удалось определить отправителя");
        await Clients.All.SendAsync("ReceiveMessage", userName, message, DateTimeOffset.UtcNow);
    }
}
