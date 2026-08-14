using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DevForge.Web.Controllers;

/// <summary>
/// Controller for routing to developer utility views.
/// </summary>
public class ToolsController : Controller
{
    [HttpGet]
    public IActionResult JsonFormatter()
    {
        return View();
    }

    [HttpGet]
    public IActionResult JwtInspector()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SqlFormatter()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult ApiPlayground()
    {
        return View();
    }
}
