using Microsoft.AspNetCore.Mvc;

namespace Kt20Chat.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }
}
