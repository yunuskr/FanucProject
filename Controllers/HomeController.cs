using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Data;
using Microsoft.Data.SqlClient;
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
    public IActionResult Index(string username)
    {
        
        return View();
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

    public IActionResult ProgramDetails(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        // Program detaylarını ViewBag ile gönder
        ViewBag.ProgramId = id;
        
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveSettings(Setting model)
    {
        if (ModelState.IsValid)
        {
            // Eğer her zaman tek bir kayıt tutacaksan (Id = 1 gibi)
            var setting = _context.Settings.FirstOrDefault();
            if (setting == null)
            {
                _context.Settings.Add(model);
            }
            else
            {
                setting.RobotIp = model.RobotIp;
                setting.RobotUser = model.RobotUser;
                setting.RobotPassword = model.RobotPassword;
                setting.SqlIp = model.SqlIp;
                setting.Database = model.Database;
                setting.SqlUser = model.SqlUser;
                setting.SqlPassword = model.SqlPassword;
                setting.TrustServerCertificate = model.TrustServerCertificate;

                _context.Settings.Update(setting);
            }

            _context.SaveChanges();
            TempData["Success"] = "Ayarlar başarıyla kaydedildi.";
        }
        else
        {
            TempData["Error"] = "Eksik alanlar var!";
        }

        return RedirectToAction("Index", "Home");
    }
    public IActionResult TestDynamicDb()
{
    var dynamicConn = ConnectionHelper.GetDynamicConnection(_context);

    if (string.IsNullOrEmpty(dynamicConn))
    {
        return Content("Ayarlar bulunamadı!");
    }

    using (var conn = new SqlConnection(dynamicConn))
    {
        conn.Open();
        using (var cmd = new SqlCommand("SELECT TOP 1 name FROM sys.tables", conn))
        {
            var result = cmd.ExecuteScalar();
            return Content($"Bağlantı başarılı ✅, İlk tablo: {result}");
        }
    }
}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
