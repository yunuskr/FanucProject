using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.Data;
using FanucRelease.Services.Interfaces;
using FanucRelease.ViewModels;

namespace FanucRelease.Controllers
{
    public class RaporlarController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IOperatorService _operatorService;
        private readonly IProgramVerisiService _programService;
        private readonly IHataService _hataService;

        public RaporlarController(
            ApplicationDbContext db,
            IOperatorService operatorService,
            IProgramVerisiService programService,
            IHataService hataService)
        {
            _db = db;
            _operatorService = operatorService;
            _programService = programService;
            _hataService = hataService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? operatorId)
        {
            // Varsayılan: son 30 gün
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Today.AddDays(1).AddTicks(-1);
            var periodDays = Math.Max(1, (int)Math.Ceiling((end - start).TotalDays));
            var prevStart = start.AddDays(-periodDays);
            var prevEnd = start.AddTicks(-1);

            // Programlar filtreli
            var progQuery = _db.ProgramVerileri.AsNoTracking();
            progQuery = progQuery.Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= start
                                           && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= end);
            if (operatorId.HasValue)
                progQuery = progQuery.Where(p => p.OperatorId == operatorId.Value);

            var programs = await progQuery
                .Include(p => p.Kaynaklar)
                .Include(p => p.Hatalar)
                .Include(p => p.Operator)
                .ToListAsync();

            // Summary metrics
            var vm = new RaporlarIndexVM
            {
                StartDate = start,
                EndDate = end,
                OperatorId = operatorId
            };

            vm.ToplamProgram = programs.Count;
            vm.ToplamKaynak = programs.Sum(p => p.Kaynaklar?.Count ?? 0);
            vm.BasariliKaynak = programs.Sum(p => p.Kaynaklar?.Count(k => k.basarili_mi) ?? 0);
            vm.ToplamHata = programs.Sum(p => p.Hatalar?.Count ?? 0);

            // Top Operators (reuse OperatorService with days filter)
            var days = (int)Math.Ceiling((end - start).TotalDays);
            var opRows = await _operatorService.GetOperatorPerformanceAsync(minProgram: 0, days: days);
            if (operatorId.HasValue)
                opRows = opRows.Where(o => o.OperatorId == operatorId.Value).ToList();
            vm.TopOperatorlar = opRows
                .OrderByDescending(o => o.ProgramSayisi)
                .ThenByDescending(o => o.BasariYuzdesi)
                .Take(5)
                .ToList();

            // Top Programs (by kaynak sayısı)
            var gecmisRows = programs
                .OrderByDescending(p => p.Kaynaklar?.Count ?? 0)
                .Take(10)
                .Select(p => new FanucRelease.Models.ViewModels.GecmisProgramRow
                {
                    Id = p.Id,
                    ProgramAdi = p.ProgramAdi ?? "—",
                    KaynakSayisi = p.Kaynaklar?.Count ?? 0,
                    OperatorAdSoyad = p.Operator is null ? "—" : ($"{p.Operator.Ad} {p.Operator.Soyad}").Trim(),
                    ToplamSureText = "—",
                    TarihText = (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih).ToString("dd.MM.yyyy"),
                    BasariYuzde = (p.Kaynaklar?.Count ?? 0) > 0 ? (int)Math.Round((p.Kaynaklar!.Count(k => k.basarili_mi) * 100.0) / (p.Kaynaklar!.Count)) : 0,
                    BadgeClass = (p.Kaynaklar?.Count ?? 0) >= 25 ? "badge badge-success"
                               : (p.Kaynaklar?.Count ?? 0) >= 15 ? "badge badge-warning" : "badge badge-danger"
                })
                .ToList();
            vm.TopProgramlar = gecmisRows;

            // Hata trend (gün bazlı sayım)
            vm.HataTrend = programs
                .SelectMany(p => p.Hatalar ?? new List<Models.Hata>())
                .GroupBy(h => h.Zaman.Date)
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.Count()))
                .ToList();

            // Uzun süren programlar (tahmini süre: Bitiş - Başlangıç veya 0)
            vm.UzunSurenProgramlar = programs
                .Select(p => new ViewModels.ProgramDurationRow
                {
                    ProgramId = p.Id,
                    ProgramAdi = p.ProgramAdi,
                    TarihText = (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih).ToString("dd.MM.yyyy"),
                    OperatorAdSoyad = p.Operator == null ? null : ($"{p.Operator.Ad} {p.Operator.Soyad}").Trim(),
                    KaynakSayisi = p.Kaynaklar?.Count ?? 0,
                    BasariYuzdesi = (p.Kaynaklar?.Count ?? 0) > 0 ? (int)Math.Round((p.Kaynaklar!.Count(k => k.basarili_mi) * 100.0) / (p.Kaynaklar!.Count)) : 0,
                    Sure = (p.BitisZamani > DateTime.MinValue && p.BaslangicZamani > DateTime.MinValue && p.BitisZamani > p.BaslangicZamani)
                        ? (p.BitisZamani - p.BaslangicZamani)
                        : TimeSpan.Zero
                })
                .OrderByDescending(x => x.Sure)
                .Take(10)
                .ToList();

            // Durus Ozeti yerine: Karşılaştırma (daily/weekly/monthly/yearly)
            // Default: weekly current week (Mon-Sat) vs previous week
            string compareMode = HttpContext.Request.Query["compareMode"].FirstOrDefault() ?? "weekly";
            DateTime? compareABase = null;
            DateTime? compareBBase = null;
            if (DateTime.TryParse(HttpContext.Request.Query["compareABase"].FirstOrDefault(), out var aParsed)) compareABase = aParsed;
            if (DateTime.TryParse(HttpContext.Request.Query["compareBBase"].FirstOrDefault(), out var bParsed)) compareBBase = bParsed;

            // New weekly selection inputs: AYear/AMonth/AWeek and BYear/BMonth/BWeek
            int.TryParse(HttpContext.Request.Query["AYear"].FirstOrDefault(), out var aYear);
            int.TryParse(HttpContext.Request.Query["AMonth"].FirstOrDefault(), out var aMonth);
            int.TryParse(HttpContext.Request.Query["AWeek"].FirstOrDefault(), out var aWeek);
            int.TryParse(HttpContext.Request.Query["BYear"].FirstOrDefault(), out var bYear);
            int.TryParse(HttpContext.Request.Query["BMonth"].FirstOrDefault(), out var bMonth);
            int.TryParse(HttpContext.Request.Query["BWeek"].FirstOrDefault(), out var bWeek);

            (DateTime from, DateTime to, string label) ResolveWindow(DateTime baseDate)
            {
                switch (compareMode)
                {
                    case "daily":
                        var d0 = baseDate.Date;
                        return (d0, d0.AddDays(1).AddTicks(-1), d0.ToString("dd.MM.yyyy"));
                    case "monthly":
                        var m0 = new DateTime(baseDate.Year, baseDate.Month, 1);
                        var m1 = m0.AddMonths(1).AddTicks(-1);
                        return (m0, m1, m0.ToString("MMMM yyyy"));
                    case "yearly":
                        var y0 = new DateTime(baseDate.Year, 1, 1);
                        var y1 = y0.AddYears(1).AddTicks(-1);
                        return (y0, y1, baseDate.Year.ToString());
                    case "weekly":
                    default:
                        // Mon-Sat window
                        int diff = ((int)baseDate.DayOfWeek + 6) % 7; // Monday=0
                        var w0 = baseDate.Date.AddDays(-diff);
                        var wEnd = w0.AddDays(5); // Monday..Saturday
                        return (w0, wEnd.AddDays(1).AddTicks(-1), $"{w0:dd.MM} - {wEnd:dd.MM}");
                }
            }

            var today = DateTime.Today;

            // Helper: compute Mon-Sat week by (year, month, weekIndex 1..4). Week 1 starts at the first Monday in the month,
            // each week spans Mon..Sat; if requested week spills past month end, clamp to month end.
            (DateTime from, DateTime to, string label)? MonthWeek(int year, int month, int weekIdx)
            {
                if (year <= 0 || month <= 0 || month > 12 || weekIdx <= 0) return null;
                var firstDay = new DateTime(year, month, 1);
                int diff = ((int)firstDay.DayOfWeek + 6) % 7; // Monday=0
                var firstMonday = firstDay.AddDays(diff == 0 ? 0 : (7 - diff));
                var start = firstMonday.AddDays((weekIdx - 1) * 7);
                var monthEnd = firstDay.AddMonths(1).AddTicks(-1);
                var end = start.AddDays(5); // Mon..Sat
                if (start.Month != month) return null; // week before month start
                if (start > monthEnd) return null;
                if (end > monthEnd.Date) end = monthEnd.Date;
                var lbl = $"{start:dd.MM} - {end:dd.MM} ({firstDay:MMMM yyyy})";
                return (start, end.AddDays(1).AddTicks(-1), lbl);
            }

            (DateTime curFrom, DateTime curTo, string curLabel) cur;
            (DateTime prevFrom, DateTime prevTo, string prevLabel) prev;

            if (compareMode == "weekly" && aYear > 0 && aMonth > 0 && aWeek > 0)
            {
                var aWeekRange = MonthWeek(aYear, aMonth, aWeek);
                if (aWeekRange.HasValue)
                {
                    cur = aWeekRange.Value;
                }
                else
                {
                    cur = ResolveWindow(compareABase ?? today);
                }

                if (bYear > 0 && bMonth > 0 && bWeek > 0)
                {
                    var bWeekRange = MonthWeek(bYear, bMonth, bWeek);
                    prev = bWeekRange ?? ResolveWindow(compareBBase ?? today.AddDays(-7));
                }
                else
                {
                    prev = compareABase.HasValue
                        ? ResolveWindow(compareBBase ?? today.AddDays(-7))
                        : ResolveWindow(today.AddDays(-7));
                }
            }
            else
            {
                var _cur = ResolveWindow(compareABase ?? today);
                var _prev = compareABase.HasValue
                    ? ResolveWindow(compareBBase ?? (compareMode == "weekly" ? today.AddDays(-7) : today))
                    : compareMode switch
                    {
                        "daily" => ResolveWindow(today.AddDays(-1)),
                        "monthly" => ResolveWindow(today.AddMonths(-1)),
                        "yearly" => ResolveWindow(today.AddYears(-1)),
                        _ => ResolveWindow(today.AddDays(-7))
                    };
                cur = _cur; prev = _prev;
            }

            async Task<(int Program, int Kaynak, int Hata, int SureDakika)> ComputeWindowAsync(DateTime f, DateTime t)
            {
                var q = _db.ProgramVerileri.AsNoTracking()
                    .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= f
                             && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= t);
                if (operatorId.HasValue) q = q.Where(p => p.OperatorId == operatorId.Value);
                var rows = await q.Include(p => p.Kaynaklar).Include(p => p.Hatalar).ToListAsync();
                var prg = rows.Count;
                var kyn = rows.Sum(p => p.Kaynaklar?.Count ?? 0);
                var hta = rows.Sum(p => p.Hatalar?.Count ?? 0);
                var dur = rows.Sum(p => (p.BitisZamani > DateTime.MinValue && p.BaslangicZamani > DateTime.MinValue && p.BitisZamani > p.BaslangicZamani)
                    ? (int)Math.Round((p.BitisZamani - p.BaslangicZamani).TotalMinutes)
                    : 0);
                return (prg, kyn, hta, dur);
            }

            var aStats = await ComputeWindowAsync(cur.curFrom, cur.curTo);
            var bStats = await ComputeWindowAsync(prev.prevFrom, prev.prevTo);
            vm.CompareMode = compareMode;
            vm.CompareABase = cur.curFrom;
            vm.CompareBBase = prev.prevFrom;
            vm.CompareA = (cur.curLabel, aStats.Program, aStats.Kaynak, aStats.Hata, aStats.SureDakika);
            vm.CompareB = (prev.prevLabel, bStats.Program, bStats.Kaynak, bStats.Hata, bStats.SureDakika);
            vm.CompareDelta = (aStats.Program - bStats.Program, aStats.Kaynak - bStats.Kaynak, aStats.Hata - bStats.Hata, aStats.SureDakika - bStats.SureDakika);

            // Persist selections onto VM so the UI can keep selections
            if (aYear > 0) vm.AYear = aYear; else vm.AYear = today.Year;
            if (aMonth > 0) vm.AMonth = aMonth; else vm.AMonth = today.Month;
            if (aWeek > 0) vm.AWeek = aWeek; else vm.AWeek = 1;
            if (bYear > 0) vm.BYear = bYear; else vm.BYear = (today.AddDays(-7)).Year;
            if (bMonth > 0) vm.BMonth = bMonth; else vm.BMonth = (today.AddDays(-7)).Month;
            if (bWeek > 0) vm.BWeek = bWeek; else vm.BWeek = 1;

            // Previous period metrics for delta comparisons
            var prevQuery = _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= prevStart
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= prevEnd);
            if (operatorId.HasValue)
                prevQuery = prevQuery.Where(p => p.OperatorId == operatorId.Value);
            var prevPrograms = await prevQuery
                .Include(p => p.Kaynaklar)
                .Include(p => p.Hatalar)
                .ToListAsync();

            vm.OncekiToplamProgram = prevPrograms.Count;
            vm.OncekiToplamKaynak = prevPrograms.Sum(p => p.Kaynaklar?.Count ?? 0);
            vm.OncekiBasariliKaynak = prevPrograms.Sum(p => p.Kaynaklar?.Count(k => k.basarili_mi) ?? 0);
            vm.OncekiToplamHata = prevPrograms.Sum(p => p.Hatalar?.Count ?? 0);

            // Daily trend for current period
            vm.GunlukTrend = programs
                .GroupBy(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani.Date : p.Tarih.Date))
                .OrderBy(g => g.Key)
                .Select(g => (
                    Date: g.Key,
                    Program: g.Count(),
                    Kaynak: g.Sum(p => p.Kaynaklar?.Count ?? 0),
                    BasariYuzdesi: (g.Sum(p => p.Kaynaklar?.Count ?? 0) > 0)
                        ? (int)Math.Round((g.Sum(p => p.Kaynaklar!.Count(k => k.basarili_mi)) * 100.0) / (g.Sum(p => p.Kaynaklar!.Count)))
                        : 0
                ))
                .ToList();

            // Top errors by code/description in current period
            vm.TopHatalar = programs
                .SelectMany(p => p.Hatalar ?? new List<Models.Hata>())
                .GroupBy(h => new { h.Kod, h.Aciklama })
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => (g.Key.Kod, g.Key.Aciklama, g.Count()))
                .ToList();

            return View(vm);
        }
    }
}

