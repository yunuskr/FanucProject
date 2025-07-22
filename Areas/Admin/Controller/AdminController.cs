using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Admin/[action]")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View(); // --> Views/AdminHome/Index.cshtml çağrılır
        }
    }
}
