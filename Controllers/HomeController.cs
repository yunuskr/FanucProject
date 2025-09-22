using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Models.ViewModels;

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
        var vm = await BuildDashboardViewModelAsync();
        return View(vm);
    }

    // (Home'a POST dönüşleri varsa tablo boş kalmasın)
    [HttpPost]
    public async Task<IActionResult> Index(string? username)
    {
        var vm = await BuildDashboardViewModelAsync();
        return View(vm);
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
    private async Task<DashboardViewModel> BuildDashboardViewModelAsync()
    {
        var vm = new DashboardViewModel();

        // History rows and counts
        vm.GecmisProgramlar = await _programService.GetGecmisProgramlarRowsAsync();
        vm.ProgramCount = await _programService.GetTotalProgramCountAsync();

        // Latest program and latest successful kaynak
        vm.SonProgram = await _programService.GetLastProgramAsync();
        vm.SonBasariliKaynak = await _kaynakService.GetLastSuccessfulKaynakOfLatestProgramAsync();

        // Global kaynak counts
        var counts = await _kaynakService.GetKaynakCountsAsync();
        var toplam = counts.toplam;
        var basarili = counts.basarili;
        vm.ToplamKaynak = toplam;
        vm.BasariliKaynak = basarili;

        // Total errors
        vm.HataToplam = await _hataService.GetToplamHataCountAsync();

        // Today total time
        var bugunSure = await _kaynakService.GetBugunToplamSureAsync();
        vm.BugunToplamKaynakSureText = FormatTimeSpan(bugunSure);

        // Last program quick stats (avoid heavy Razor)
        if (vm.SonProgram != null)
        {
            var kaynaklar = vm.SonProgram.Kaynaklar?.OrderBy(k => k.BaslangicSaati).ToList() ?? new List<Kaynak>();
            var bas = (vm.SonProgram.BaslangicZamani > DateTime.MinValue)
                        ? vm.SonProgram.BaslangicZamani
                        : (kaynaklar.Count > 0 ? kaynaklar.First().BaslangicSaati : (DateTime?)null);
            var bit = (vm.SonProgram.BitisZamani > DateTime.MinValue)
                        ? vm.SonProgram.BitisZamani
                        : (kaynaklar.Count > 0 ? kaynaklar.Max(x => x.BitisSaati) : (DateTime?)null);
            vm.SonProgramBaslangicText = bas?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—";
            vm.SonProgramBitisText = bit?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—";

            vm.SonProgramToplam = kaynaklar.Count;
            vm.SonProgramBasarili = kaynaklar.Count(k => k.basarili_mi);
            vm.SonProgramBasarisiz = Math.Max(0, vm.SonProgramToplam - vm.SonProgramBasarili);
            vm.SonProgramBasariYuzde = vm.SonProgramToplam > 0
                ? (int)Math.Round(vm.SonProgramBasarili * 100.0 / vm.SonProgramToplam)
                : 0;

            if (vm.SonBasariliKaynak != null)
            {
                var ts = vm.SonBasariliKaynak.ToplamSure != default
                    ? vm.SonBasariliKaynak.ToplamSure.ToTimeSpan()
                    : (vm.SonBasariliKaynak.BitisSaati - vm.SonBasariliKaynak.BaslangicSaati);
                if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;
                vm.SonBasariliKaynakToplamSureText = ts.Hours > 0
                    ? $"{(int)ts.TotalHours} sa {ts.Minutes} dk {ts.Seconds} sn"
                    : $"{ts.Minutes} dk {ts.Seconds} sn";
            }
        }

        return vm;
    }
}
