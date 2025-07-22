using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Data;
namespace FanucRelease.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Index(string username, string userpassword, bool rememberMe)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.KullaniciAdi == username && u.Sifre == userpassword);

        if (user != null)
        {
            // Cookie ayarı (beni hatırla)
            if (rememberMe)
            {
                Response.Cookies.Append("rememberedUser", username, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30)
                });
            }
            else
            {
                Response.Cookies.Delete("rememberedUser");
            }

            // Giriş başarılı, yönlendir
            return RedirectToAction("Index", "Home"); // veya başka bir sayfa
        }
        else
        {
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
