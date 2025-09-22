using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;

namespace FanucRelease.Controllers;

public class HomeController : Controller
{
    private readonly IProgramVerisiService _programService;
    private readonly ISettingsService _settingsService;
    private readonly IKaynakService _kaynakService;
    private readonly IHataService _hataService;

    public HomeController(IProgramVerisiService programService, ISettingsService settingsService, IKaynakService kaynakService, IHataService hataService)
    {
        _programService = programService;
        _settingsService = settingsService;
        _kaynakService = kaynakService;
        _hataService = hataService;
    }

    // Artık geçmiş program satırları servis tarafından sağlanıyor

    // ----------------- Home / Index -----------------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.GecmisProgramlar = await _programService.GetGecmisProgramlarRowsAsync();
        ViewBag.ProgramCount = await _programService.GetTotalProgramCountAsync();
        // En son program
        var lastProgram = await _programService.GetLastProgramAsync();

    // En son programın son başarılı kaynağı
    ViewBag.SonBasariliKaynak = await _kaynakService.GetLastSuccessfulKaynakOfLatestProgramAsync();

        // Kaynak sayıları (global toplamlardan)
        {
            var (toplam, basarili) = await _kaynakService.GetKaynakCountsAsync();
            ViewBag.KaynakToplam = toplam;
            ViewBag.KaynakBasarili = basarili;
        }

        // Toplam Hata sayısı
        ViewBag.HataToplam = await _hataService.GetToplamHataCountAsync();

        // Bugün toplam kaynak süresi (saat/dakika/saniye)
        var bugunSure = await _kaynakService.GetBugunToplamSureAsync();
        ViewBag.AktifSureStr = FormatTimeSpan(bugunSure);

        return View(lastProgram);
  
    }

    // (Home'a POST dönüşleri varsa tablo boş kalmasın)
    [HttpPost]
    public async Task<IActionResult> Index(string? username)
    {
        ViewBag.GecmisProgramlar = await _programService.GetGecmisProgramlarRowsAsync();
        ViewBag.ProgramCount = await _programService.GetTotalProgramCountAsync();
        var lastProgram = await _programService.GetLastProgramAsync();

        // En son programın son başarılı kaynağı
        ViewBag.SonBasariliKaynak = await _kaynakService.GetLastSuccessfulKaynakOfLatestProgramAsync();

        {
            var (toplam, basarili) = await _kaynakService.GetKaynakCountsAsync();
            ViewBag.KaynakToplam = toplam;
            ViewBag.KaynakBasarili = basarili;
        }
        // Toplam Hata sayısı
        ViewBag.HataToplam = await _hataService.GetToplamHataCountAsync();
        var bugunSure2 = await _kaynakService.GetBugunToplamSureAsync();
        ViewBag.AktifSureStr = FormatTimeSpan(bugunSure2);
        return View(lastProgram);
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        // Her zaman saat, dakika ve saniye göster: "18 sa 45 dk 37 sn"
        int hours = (int)Math.Floor(ts.TotalHours);
        int minutes = ts.Minutes;
        int seconds = ts.Seconds;
        return $"{hours} sa {minutes} dk {seconds} sn";
    }

    // ----------------- (Opsiyonel) Ayrı sayfa -----------------
    [HttpGet]
    [Route("gecmis-programlar")]
    public async Task<IActionResult> GecmisProgramlar()
    {
    ViewBag.GecmisProgramlar = await _programService.GetGecmisProgramlarRowsAsync();
        return View();
    }

    public IActionResult Privacy() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(Setting model)
    {
        if (ModelState.IsValid)
        {
            await _settingsService.SaveOrUpdateAsync(model);
            TempData["Success"] = "Ayarlar başarıyla kaydedildi.";
        }
        else
        {
            TempData["Error"] = "Eksik alanlar var!";
        }

        return RedirectToAction("Index", "Home");
    }
    

    // TestDynamicDb kaldırıldı (geçici yardımcıydı)

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
