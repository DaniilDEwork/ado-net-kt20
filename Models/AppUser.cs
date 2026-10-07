namespace Kt20Chat.Models;

public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string NormalizedUserName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
