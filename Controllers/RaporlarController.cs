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

            // Durus placeholder (ileride MakineDurus üzerinden hesaplanacak)
            vm.ToplamDurusAdet = 0;
            vm.ToplamDurusSuresi = TimeSpan.Zero;

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

