using Microsoft.AspNetCore.Mvc;

namespace Kt20Chat.Controllers;

public class AccountController : Controller
{
    public IActionResult Register() => View();

    public IActionResult Login(string? returnUrl, string? reason)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : "/Chat";
        ViewData["Expired"] = reason == "expired";
        ViewData["ChatRequired"] = returnUrl == "/Chat";
        return View();
    }
}
