using Microsoft.AspNetCore.Mvc;

namespace Kt20Chat.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ChatController : Controller
{
    public IActionResult Index() => View();
}
