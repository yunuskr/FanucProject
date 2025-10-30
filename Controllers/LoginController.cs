using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.Data;
using FanucRelease.Services;

namespace FanucRelease.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        public LoginController(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }


        [HttpGet]
        public IActionResult Index() => View();  // Views/Login/Index.cshtml

        // --- Kullanıcı girişi (sadece username) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["UserError"] = "Kullanıcı adı boş olamaz.";
                return View();
            }

            // Not: Normal kullanıcılar tablo adı sende çoğunlukla Operators.
            var user = await _context.Operators
                .AsNoTracking()
                .Where(o => o.KullaniciAdi == username)
                .Select(o => new { o.Id, o.Ad, o.Soyad, o.KullaniciAdi })
                .FirstOrDefaultAsync();

            if (user is null)
            {
                TempData["UserError"] = "Kullanıcı bulunamadı.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{user.Ad} {user.Soyad}".Trim()),
                new Claim("Username", user.KullaniciAdi),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("UserId", user.Id.ToString())
            };

            await SignInAsync(claims);
            return RedirectToAction("Index", "Home");
        }

        // --- Admin girişi (modal) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(string adminUsername, string adminPassword)
        {
            if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
            {
                TempData["AdminError"] = "Kullanıcı adı veya şifre boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            var admin = await _context.Admins
                .AsNoTracking()
                .Where(a => a.KullaniciAdi == adminUsername && a.Sifre == adminPassword)
                .Select(a => new { a.Id, a.KullaniciAdi })
                .FirstOrDefaultAsync();

            if (admin is null)
            {
                TempData["AdminError"] = "Kullanıcı adı veya şifre yanlış.";
                return RedirectToAction(nameof(Index));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.KullaniciAdi),
                new Claim("Username", admin.KullaniciAdi),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("AdminId", admin.Id.ToString())
            };

            await SignInAsync(claims);
            return RedirectToAction("Index", "Admin", new { area = "Admin" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task SignInAsync(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 🎯 Kullanıcıyı CurrentUserService içine yaz
            _currentUser.SetFromClaims(principal);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }
    }
}
