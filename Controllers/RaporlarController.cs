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

            // Per-section date ranges (defaults to global)
            DateTime trendFrom = start, trendTo = end;
            DateTime errorFrom = start, errorTo = end;
            DateTime opFrom = start, opTo = end;
            DateTime progFrom = start, progTo = end;
            DateTime durFrom = start, durTo = end;

            if (DateTime.TryParse(HttpContext.Request.Query["trendStart"].FirstOrDefault(), out var tS)) trendFrom = tS;
            if (DateTime.TryParse(HttpContext.Request.Query["trendEnd"].FirstOrDefault(), out var tE)) trendTo = tE.Date.AddDays(1).AddTicks(-1);
            if (DateTime.TryParse(HttpContext.Request.Query["errorStart"].FirstOrDefault(), out var eS)) errorFrom = eS;
            if (DateTime.TryParse(HttpContext.Request.Query["errorEnd"].FirstOrDefault(), out var eE)) errorTo = eE.Date.AddDays(1).AddTicks(-1);
            if (DateTime.TryParse(HttpContext.Request.Query["opStart"].FirstOrDefault(), out var oS)) opFrom = oS;
            if (DateTime.TryParse(HttpContext.Request.Query["opEnd"].FirstOrDefault(), out var oE)) opTo = oE.Date.AddDays(1).AddTicks(-1);
            if (DateTime.TryParse(HttpContext.Request.Query["progStart"].FirstOrDefault(), out var pS)) progFrom = pS;
            if (DateTime.TryParse(HttpContext.Request.Query["progEnd"].FirstOrDefault(), out var pE)) progTo = pE.Date.AddDays(1).AddTicks(-1);
            if (DateTime.TryParse(HttpContext.Request.Query["durStart"].FirstOrDefault(), out var dS)) durFrom = dS;
            if (DateTime.TryParse(HttpContext.Request.Query["durEnd"].FirstOrDefault(), out var dE)) durTo = dE.Date.AddDays(1).AddTicks(-1);

            vm.TrendStartDate = trendFrom.Date;
            vm.TrendEndDate = trendTo;
            vm.ErrorStartDate = errorFrom.Date;
            vm.ErrorEndDate = errorTo;
            vm.OpStartDate = opFrom.Date;
            vm.OpEndDate = opTo;
            vm.ProgStartDate = progFrom.Date;
            vm.ProgEndDate = progTo;
            vm.DurStartDate = durFrom.Date;
            vm.DurEndDate = durTo;

            // Top Operators using explicit date range
            var opRows = await _operatorService.GetOperatorPerformanceAsync(minProgram: 0, days: 0);
            // Filter rows by explicit date range by recomputing from DB would be ideal; use service overload if available
            // Fallback: recompute quickly over range
            var opQuery = _db.ProgramVerileri.AsNoTracking()
                .Where(p => p.OperatorId != null)
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= opFrom && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= opTo);
            if (operatorId.HasValue) opQuery = opQuery.Where(p => p.OperatorId == operatorId.Value);
            var opFlat = await opQuery
                .Select(p => new
                {
                    p.OperatorId,
                    OpAd = p.Operator!.Ad,
                    OpSoyad = p.Operator.Soyad,
                    OpUser = p.Operator.KullaniciAdi,
                    KaynakToplam = p.Kaynaklar.Count,
                    KaynakBasarili = p.Kaynaklar.Count(k => k.basarili_mi),
                    HataSayisi = p.Hatalar.Count,
                    BitisOrFallback = p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih,
                    SureTicks = (p.BitisZamani > DateTime.MinValue && p.BaslangicZamani > DateTime.MinValue && p.BitisZamani > p.BaslangicZamani)
                        ? (long?)(p.BitisZamani - p.BaslangicZamani).Ticks : null
                })
                .ToListAsync();
            var opDict = new Dictionary<int, (string Ad,string Soyad,string Kullanici,int Program,int Kaynak,int Basarili,int Hata,DateTime? Son,long TopSure,int SureAdet)>();
            foreach (var f in opFlat)
            {
                if (!f.OperatorId.HasValue) continue;
                var id = f.OperatorId.Value;
                if (!opDict.TryGetValue(id, out var acc)) acc = (f.OpAd ?? string.Empty, f.OpSoyad ?? string.Empty, f.OpUser ?? string.Empty,0,0,0,0,null,0L,0);
                acc.Program += 1;
                acc.Kaynak += f.KaynakToplam;
                acc.Basarili += f.KaynakBasarili;
                acc.Hata += f.HataSayisi;
                if (acc.Son == null || f.BitisOrFallback > acc.Son) acc.Son = f.BitisOrFallback;
                if (f.SureTicks.HasValue) { acc.TopSure += f.SureTicks.Value; acc.SureAdet += 1; }
                opDict[id] = acc;
            }
            vm.TopOperatorlar = opDict.Select(kv => new FanucRelease.ViewModels.OperatorPerformanceRow
            {
                OperatorId = kv.Key,
                KullaniciAdi = kv.Value.Kullanici,
                AdSoyad = ($"{kv.Value.Ad} {kv.Value.Soyad}").Trim(),
                ProgramSayisi = kv.Value.Program,
                ToplamKaynak = kv.Value.Kaynak,
                BasariliKaynak = kv.Value.Basarili,
                HataSayisi = kv.Value.Hata,
                SonProgramZamani = kv.Value.Son,
                OrtalamaProgramSuresi = kv.Value.SureAdet > 0 ? TimeSpan.FromTicks(kv.Value.TopSure / kv.Value.SureAdet) : null
            })
            .OrderByDescending(o => o.ProgramSayisi)
            .ThenByDescending(o => o.BasariYuzdesi)
            .Take(5)
            .ToList();

            // Top Programs (by kaynak) for range
            var progQuery2 = _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= progFrom && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= progTo);
            if (operatorId.HasValue) progQuery2 = progQuery2.Where(p => p.OperatorId == operatorId.Value);
            var progPrograms = await progQuery2.Include(p => p.Kaynaklar).Include(p => p.Hatalar).Include(p => p.Operator).ToListAsync();
            vm.TopProgramlar = progPrograms
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

            // Hata trend over range
            var errorPrograms = await _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= errorFrom && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= errorTo)
                .Include(p => p.Hatalar)
                .ToListAsync();
            vm.HataTrend = errorPrograms
                .SelectMany(p => p.Hatalar ?? new List<Models.Hata>())
                .GroupBy(h => h.Zaman.Date)
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.Count()))
                .ToList();

            // Uzun süren programlar over range
            var durPrograms = await _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= durFrom && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= durTo)
                .Include(p => p.Kaynaklar)
                .Include(p => p.Operator)
                .ToListAsync();
            vm.UzunSurenProgramlar = durPrograms
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

            // Helpers for explicit month/year picks
            (DateTime from, DateTime to, string label) MonthRange(int year, int month)
            {
                var m0 = new DateTime(year, month, 1);
                var m1 = m0.AddMonths(1).AddTicks(-1);
                return (m0, m1, m0.ToString("MMMM yyyy"));
            }
            (DateTime from, DateTime to, string label) YearRange(int year)
            {
                var y0 = new DateTime(year, 1, 1);
                var y1 = y0.AddYears(1).AddTicks(-1);
                return (y0, y1, year.ToString());
            }

            if (compareMode == "weekly")
            {
                if (aYear > 0 && aMonth > 0 && aWeek > 0)
                {
                    var aWeekRange = MonthWeek(aYear, aMonth, aWeek);
                    cur = aWeekRange ?? ResolveWindow(compareABase ?? today);
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
                    // If explicit A provided but not B, fallback to previous week
                    prev = ResolveWindow((compareABase ?? today).AddDays(-7));
                }
            }
            else if (compareMode == "monthly")
            {
                if (aYear > 0 && aMonth > 0)
                {
                    cur = MonthRange(aYear, aMonth);
                }
                else
                {
                    cur = ResolveWindow(compareABase ?? today);
                }

                if (bYear > 0 && bMonth > 0)
                {
                    prev = MonthRange(bYear, bMonth);
                }
                else if (aYear > 0 && aMonth > 0)
                {
                    var prevMonthBase = new DateTime(aYear, aMonth, 1).AddMonths(-1);
                    prev = MonthRange(prevMonthBase.Year, prevMonthBase.Month);
                }
                else
                {
                    prev = ResolveWindow((compareABase ?? today).AddMonths(-1));
                }
            }
            else if (compareMode == "yearly")
            {
                if (aYear > 0)
                {
                    cur = YearRange(aYear);
                }
                else
                {
                    cur = ResolveWindow(compareABase ?? today);
                }

                if (bYear > 0)
                {
                    prev = YearRange(bYear);
                }
                else if (aYear > 0)
                {
                    prev = YearRange(aYear - 1);
                }
                else
                {
                    prev = ResolveWindow((compareABase ?? today).AddYears(-1));
                }
            }
            else // daily
            {
                var _cur = ResolveWindow(compareABase ?? today);
                var _prev = ResolveWindow((compareBBase ?? (compareABase?.AddDays(-1) ?? today.AddDays(-1))));
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

            // Daily trend using explicit trend date range
            var trendPrograms = await _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= trendFrom
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= trendTo)
                .Include(p => p.Kaynaklar)
                .ToListAsync();
            vm.GunlukTrend = trendPrograms
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

        [HttpGet]
        public async Task<IActionResult> DailyTrendPartial(DateTime? trendStart, DateTime? trendEnd, int? operatorId)
        {
            var from = trendStart ?? DateTime.Today.AddDays(-30);
            var to = (trendEnd ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var baseQuery = _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= from
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= to);
            if (operatorId.HasValue) baseQuery = baseQuery.Where(p => p.OperatorId == operatorId.Value);
            var list = await baseQuery.Include(p => p.Kaynaklar).ToListAsync();
            var rows = list
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
            return PartialView("_DailyTrend", rows);
        }

        [HttpGet]
        public async Task<IActionResult> TopOperatorsPartial(DateTime? opStart, DateTime? opEnd, int? operatorId)
        {
            var from = opStart ?? DateTime.Today.AddDays(-30);
            var to = (opEnd ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var opQuery = _db.ProgramVerileri.AsNoTracking()
                .Where(p => p.OperatorId != null)
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= from
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= to);
            if (operatorId.HasValue) opQuery = opQuery.Where(p => p.OperatorId == operatorId.Value);
            var flat = await opQuery
                .Select(p => new
                {
                    p.OperatorId,
                    OpAd = p.Operator!.Ad,
                    OpSoyad = p.Operator.Soyad,
                    OpUser = p.Operator.KullaniciAdi,
                    KaynakToplam = p.Kaynaklar.Count,
                    KaynakBasarili = p.Kaynaklar.Count(k => k.basarili_mi),
                    HataSayisi = p.Hatalar.Count,
                    BitisOrFallback = p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih,
                    SureTicks = (p.BitisZamani > DateTime.MinValue && p.BaslangicZamani > DateTime.MinValue && p.BitisZamani > p.BaslangicZamani)
                        ? (long?)(p.BitisZamani - p.BaslangicZamani).Ticks : null
                })
                .ToListAsync();
            var dict = new Dictionary<int, (string Ad,string Soyad,string Kullanici,int Program,int Kaynak,int Basarili,int Hata,DateTime? Son,long TopSure,int SureAdet)>();
            foreach (var f in flat)
            {
                if (!f.OperatorId.HasValue) continue;
                var id = f.OperatorId.Value;
                if (!dict.TryGetValue(id, out var acc)) acc = (f.OpAd ?? string.Empty, f.OpSoyad ?? string.Empty, f.OpUser ?? string.Empty,0,0,0,0,null,0L,0);
                acc.Program += 1;
                acc.Kaynak += f.KaynakToplam;
                acc.Basarili += f.KaynakBasarili;
                acc.Hata += f.HataSayisi;
                if (acc.Son == null || f.BitisOrFallback > acc.Son) acc.Son = f.BitisOrFallback;
                if (f.SureTicks.HasValue) { acc.TopSure += f.SureTicks.Value; acc.SureAdet += 1; }
                dict[id] = acc;
            }
            var rows = dict.Select(kv => new FanucRelease.ViewModels.OperatorPerformanceRow
            {
                OperatorId = kv.Key,
                KullaniciAdi = kv.Value.Kullanici,
                AdSoyad = ($"{kv.Value.Ad} {kv.Value.Soyad}").Trim(),
                ProgramSayisi = kv.Value.Program,
                ToplamKaynak = kv.Value.Kaynak,
                BasariliKaynak = kv.Value.Basarili,
                HataSayisi = kv.Value.Hata,
                SonProgramZamani = kv.Value.Son,
                OrtalamaProgramSuresi = kv.Value.SureAdet > 0 ? TimeSpan.FromTicks(kv.Value.TopSure / kv.Value.SureAdet) : null
            })
            .OrderByDescending(o => o.ProgramSayisi)
            .ThenByDescending(o => o.BasariYuzdesi)
            .Take(5)
            .ToList();
            return PartialView("_TopOperators", rows);
        }

        [HttpGet]
        public async Task<IActionResult> TopProgramsPartial(DateTime? progStart, DateTime? progEnd, int? operatorId)
        {
            var from = progStart ?? DateTime.Today.AddDays(-30);
            var to = (progEnd ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var query = _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= from
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= to);
            if (operatorId.HasValue) query = query.Where(p => p.OperatorId == operatorId.Value);
            var list = await query.Include(p => p.Kaynaklar).Include(p => p.Hatalar).Include(p => p.Operator).ToListAsync();
            var rows = list
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
            return PartialView("_TopPrograms", rows);
        }

        [HttpGet]
        public async Task<IActionResult> HataTrendPartial(DateTime? errorStart, DateTime? errorEnd, int? operatorId)
        {
            var from = errorStart ?? DateTime.Today.AddDays(-30);
            var to = (errorEnd ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var list = await _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= from
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= to)
                .Include(p => p.Hatalar)
                .ToListAsync();
            var trend = list
                .SelectMany(p => p.Hatalar ?? new List<Models.Hata>())
                .GroupBy(h => h.Zaman.Date)
                .OrderBy(g => g.Key)
                .Select(g => (Date: g.Key, Count: g.Count()))
                .ToList();
            return PartialView("_HataTrend", trend);
        }

        [HttpGet]
        public async Task<IActionResult> LongestProgramsPartial(DateTime? durStart, DateTime? durEnd, int? operatorId)
        {
            var from = durStart ?? DateTime.Today.AddDays(-30);
            var to = (durEnd ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var list = await _db.ProgramVerileri.AsNoTracking()
                .Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= from
                         && (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) <= to)
                .Include(p => p.Kaynaklar)
                .Include(p => p.Operator)
                .ToListAsync();
            var rows = list
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
            return PartialView("_LongestPrograms", rows);
        }
    }
}

