using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Data;
namespace FanucRelease.Controllers;

public class LoginController : Controller
{
    private readonly ILogger<LoginController> _logger;
    private readonly ApplicationDbContext _context;

    public LoginController(ILogger<LoginController> logger, ApplicationDbContext context)
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
    public IActionResult Index(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "Kullanıcı adı boş olamaz.";
            return View();
        }

        var kullanici = _context.Operators.FirstOrDefault(o => o.KullaniciAdi == username);

        if (kullanici != null)
        {
            // Kullanıcı bulunduysa yönlendir
            return RedirectToAction("Index", "Home");
        }
        else
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return View();
        }
    }
    [HttpPost]
    public IActionResult AdminLogin(string adminUsername, string adminPassword)
    {
        if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminPassword))
        {
            TempData["AdminError"] = "Kullanıcı adı veya şifre boş olamaz.";
            return RedirectToAction("Index");
        }

        var admin = _context.Admins.FirstOrDefault(a => a.KullaniciAdi == adminUsername && a.Sifre == adminPassword);

        if (admin != null)
        {
            // Giriş başarılı → Admin paneline yönlendir
            return RedirectToAction("Index", "Admin", new { area = "Admin" });
        }
        else
        {
            TempData["AdminError"] = "Kullanıcı adı veya şifre yanlış.";
            return RedirectToAction("Index");
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
