namespace Kt20Chat.Services;

public class TokenSessionStore
{
    private readonly object sync = new();
    private readonly Dictionary<string, DateTimeOffset> revoked = [];
    private readonly Dictionary<string, Dictionary<string, Action>> connections = [];

    public bool IsRevoked(string? id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        lock (sync)
        {
            if (!revoked.TryGetValue(id, out var expiresAt)) return false;
            if (expiresAt > DateTimeOffset.UtcNow) return true;
            revoked.Remove(id);
            return false;
        }
    }

    public bool AddConnection(string? tokenId, string connectionId, Action abort)
    {
        lock (sync)
        {
            if (IsRevoked(tokenId)) return false;
            if (!connections.TryGetValue(tokenId!, out var group))
            {
                group = [];
                connections[tokenId!] = group;
            }
            group[connectionId] = abort;
            return true;
        }
    }

    public void RemoveConnection(string? tokenId, string connectionId)
    {
        lock (sync)
        {
            if (tokenId is null || !connections.TryGetValue(tokenId, out var group)) return;
            group.Remove(connectionId);
            if (group.Count == 0) connections.Remove(tokenId);
        }
    }

    public void Revoke(string tokenId, DateTimeOffset expiresAt)
    {
        Action[] aborts;
        lock (sync)
        {
            revoked[tokenId] = expiresAt;
            aborts = connections.Remove(tokenId, out var group) ? group.Values.ToArray() : [];
        }
        foreach (var abort in aborts) abort();
    }
}
