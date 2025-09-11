using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Data;
using Microsoft.EntityFrameworkCore;
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

    // ----------------- Ortak yükleyici -----------------
    private async Task<List<object>> LoadGecmisProgramlarAsync(int take = 100)
    {
        // DbSet adın sende ProgramVerileri ise aynen kalsın; Programlar ise değiştir.
        var programlar = await _context.ProgramVerileri
            .AsNoTracking()
            .Include(p => p.Operator)
            .Include(p => p.Kaynaklar)
            .OrderByDescending(p => p.Id)
            .Take(take)
            .ToListAsync();

        var rows = programlar.Select(p =>
        {
            // Toplam süreyi Kaynaklar'dan topla
            TimeSpan toplam = TimeSpan.Zero;
            DateTime? maxBitis = null;

            foreach (var k in p.Kaynaklar)
            {
                var d = k.ToplamSure != default
                        ? k.ToplamSure.ToTimeSpan()
                        : (k.BitisSaati - k.BaslangicSaati);

                if (d > TimeSpan.Zero) toplam += d;
                if (maxBitis is null || k.BitisSaati > maxBitis.Value)
                    maxBitis = k.BitisSaati;
            }

            string SureToText(TimeSpan ts)
                => ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}h {ts.Minutes}m"
                 : ts.TotalMinutes >= 1 ? $"{ts.Minutes}m"
                 : $"{ts.Seconds}s";

            int kaynakSayisi = p.KaynakSayisi > 0 ? p.KaynakSayisi : p.Kaynaklar.Count;
            string badgeClass = kaynakSayisi >= 25 ? "badge badge-success"
                              : kaynakSayisi >= 15 ? "badge badge-warning"
                              :                       "badge badge-danger";

            return (object)new
            {
                p.Id,
                p.ProgramAdi,
                KaynakSayisi = kaynakSayisi,
                OperatorAdSoyad = p.Operator is null ? "—" : $"{p.Operator.Ad} {p.Operator.Soyad}",
                ToplamSureText = toplam == TimeSpan.Zero ? "—" : SureToText(toplam),
                TarihText = maxBitis?.ToString("dd.MM.yyyy") ?? "—",
                BasariYuzde = 100, // şimdilik sabit
                BadgeClass = badgeClass
            };
        }).ToList();

        return rows;
    }

    // ----------------- Home / Index -----------------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.GecmisProgramlar = await LoadGecmisProgramlarAsync();
        return View();
    }

    // (Home'a POST dönüşleri varsa tablo boş kalmasın)
    [HttpPost]
    public async Task<IActionResult> Index(string? username)
    {
        ViewBag.GecmisProgramlar = await LoadGecmisProgramlarAsync();
        return View();
    }

    // ----------------- (Opsiyonel) Ayrı sayfa -----------------
    [HttpGet]
    [Route("gecmis-programlar")]
    public async Task<IActionResult> GecmisProgramlar()
    {
        ViewBag.GecmisProgramlar = await LoadGecmisProgramlarAsync();
        return View();
    }

    public IActionResult Privacy() => View();

    public IActionResult ProgramDetails(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        ViewBag.ProgramId = id;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveSettings(Setting model)
    {
        if (ModelState.IsValid)
        {
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
        if (string.IsNullOrEmpty(dynamicConn)) return Content("Ayarlar bulunamadı!");

        using var conn = new SqlConnection(dynamicConn);
        conn.Open();
        using var cmd = new SqlCommand("SELECT TOP 1 name FROM sys.tables", conn);
        var result = cmd.ExecuteScalar();
        return Content($"Bağlantı başarılı ✅, İlk tablo: {result}");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
