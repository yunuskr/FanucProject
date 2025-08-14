using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        return View(); // Login/Index.cshtml (Layout = null)
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            TempData["Error"] = "Kullanıcı adı boş olamaz.";
            return View();
        }

        var kullanici = await _context.Operators
            .Where(o => o.KullaniciAdi == username)
            .Select(o => new { o.Id, o.KullaniciAdi })
            .FirstOrDefaultAsync();

        if (kullanici == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return View();
        }

        // Normal kullanıcı için cookie + claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("OperatorId", kullanici.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        return RedirectToAction("Index", "Home"); // _Layout ile
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(string adminUsername, string adminPassword)
    {
        if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
        {
            TempData["AdminError"] = "Kullanıcı adı veya şifre boş olamaz.";
            return RedirectToAction("Index");
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.KullaniciAdi == adminUsername && a.Sifre == adminPassword);

        if (admin == null)
        {
            TempData["AdminError"] = "Kullanıcı adı veya şifre yanlış.";
            return RedirectToAction("Index");
        }

        // Admin için cookie + claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.KullaniciAdi),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("AdminId", admin.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        // Areas/Admin/Admin/Index.cshtml ve adminlayout_de devreye girer
        return RedirectToAction("Index", "Admin", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index"); // Login sayfası
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
