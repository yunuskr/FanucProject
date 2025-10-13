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

        // Weekly Program Count Trend (this week vs last week)
        try
        {
            var (thisWeek, lastWeek) = await _programService.GetProgramCountsForThisAndLastWeekAsync();
            vm.ThisWeekProgramCount = thisWeek;
            vm.LastWeekProgramCount = lastWeek;
            if (lastWeek <= 0)
            {
                // If there were no programs last week, define 100% increase when thisWeek>0, else 0%
                vm.ProgramCountWoWPercent = thisWeek > 0 ? 100 : 0;
            }
            else
            {
                vm.ProgramCountWoWPercent = (int)Math.Round(((thisWeek - lastWeek) * 100.0) / lastWeek);
            }

            bool isUp = vm.ProgramCountWoWPercent >= 0;
            vm.ProgramCountTrendClass = isUp ? "trend-up" : "trend-down";
            vm.ProgramCountTrendIcon = isUp ? "fas fa-arrow-up" : "fas fa-arrow-down";
            // Turkish label: e.g., "+12% bu hafta" or "-5% bu hafta"
            vm.ProgramCountTrendText = $"{(isUp ? "+" : "")}{vm.ProgramCountWoWPercent}% bu hafta";
        }
        catch
        {
            // In case of any failure, keep defaults and continue
        }

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
        try
        {
            var (thisWeekHata, lastWeekHata) = await _hataService.GetHataCountsForThisAndLastWeekAsync();
            vm.ThisWeekHataCount = thisWeekHata;
            vm.LastWeekHataCount = lastWeekHata;
            if (lastWeekHata <= 0)
            {
                vm.HataWoWPercent = thisWeekHata > 0 ? 100 : 0;
            }
            else
            {
                vm.HataWoWPercent = (int)Math.Round(((thisWeekHata - lastWeekHata) * 100.0) / lastWeekHata);
            }

            // For errors, an increase is bad (down icon), a decrease is good (up icon) – but we keep red card style
            bool decreased = vm.HataWoWPercent < 0; // negative percent => this week < last week
            vm.HataTrendClass = decreased ? "trend-up" : "trend-down"; // show greenish up when decreased
            vm.HataTrendIcon = decreased ? "fas fa-arrow-down" : "fas fa-arrow-up"; // if decreased, show arrow-down icon
            // Label stays as signed percent
            var sign = vm.HataWoWPercent > 0 ? "+" : "";
            vm.HataTrendText = $"{sign}{vm.HataWoWPercent}% bu hafta";
        }
        catch { }

        // Today total time
        var bugunSure = await _kaynakService.GetBugunToplamSureAsync();
        vm.BugunToplamKaynakSureText = FormatTimeSpan(bugunSure);
        try
        {
            var dunSure = await _kaynakService.GetDunToplamSureAsync();
            // Percent change relative to yesterday (if yesterday is zero, treat as 100% if today>0 else 0)
            int pct;
            if (dunSure.TotalSeconds <= 0)
            {
                pct = bugunSure.TotalSeconds > 0 ? 100 : 0;
            }
            else
            {
                pct = (int)Math.Round(((bugunSure.TotalSeconds - dunSure.TotalSeconds) * 100.0) / dunSure.TotalSeconds);
            }
            vm.TodayTimePercent = pct;
            bool up = pct >= 0;
            vm.TodayTimeTrendClass = up ? "trend-up" : "trend-down";
            vm.TodayTimeTrendIcon = up ? "fas fa-arrow-up" : "fas fa-arrow-down";
            vm.TodayTimeTrendText = $"{(up ? "+" : "")}{pct}% bugün";
        }
        catch { }

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
