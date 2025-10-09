using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FanucRelease.Services.Interfaces;
using FanucRelease.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Controllers
{
    public class PerformanceController : Controller
    {
        private readonly IOperatorService _operatorService;
        public PerformanceController(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int minProgram = 0, int days = 0, double? maxProgramHours = null)
        {
            // Service tek satır / operatör dönecek şekilde tasarlandığı için ekstra grouping kaldırıldı.
            var rows = await _operatorService.GetOperatorPerformanceAsync(minProgram, days, maxProgramHours);
            var vm = new OperatorPerformanceIndexVM
            {
                Operatorlar = rows,
                OlusturmaZamani = DateTime.Now,
                MinProgram = minProgram,
                Days = days,
                MaxProgramHours = maxProgramHours,
                ToplamProgram = rows.Sum(r => r.ProgramSayisi),
                ToplamKaynak = rows.Sum(r => r.ToplamKaynak),
                ToplamBasariliKaynak = rows.Sum(r => r.BasariliKaynak),
                ToplamHata = rows.Sum(r => r.HataSayisi)
            };
            return View(vm);
        }

        // Yardımcı: süre ortalaması (null değerleri atla)
        private static TimeSpan? CalcAverage(IEnumerable<TimeSpan?> spans)
        {
            var vals = spans.Where(s => s.HasValue).Select(s => s!.Value).ToList();
            if (!vals.Any()) return null;
            var avgTicks = vals.Average(v => v.Ticks);
            return TimeSpan.FromTicks((long)avgTicks);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(int minProgram = 0, int days = 0, double? maxProgramHours = null)
        {
            var rows = await _operatorService.GetOperatorPerformanceAsync(minProgram, days, maxProgramHours);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("OperatorId;KullaniciAdi;AdSoyad;ProgramSayisi;ToplamKaynak;BasariliKaynak;BasarisizKaynak;BasariYuzdesi;HataSayisi;OrtSureSn;SonProgramZamani");
            foreach (var r in rows)
            {
                var ortSn = r.OrtalamaProgramSuresi?.TotalSeconds.ToString("F0") ?? "";
                sb.AppendLine(string.Join(';', new [] {
                    r.OperatorId.ToString(),
                    r.KullaniciAdi,
                    r.AdSoyad.Replace(';',','),
                    r.ProgramSayisi.ToString(),
                    r.ToplamKaynak.ToString(),
                    r.BasariliKaynak.ToString(),
                    r.BasarisizKaynak.ToString(),
                    r.BasariYuzdesi.ToString(),
                    r.HataSayisi.ToString(),
                    ortSn,
                    r.SonProgramZamani?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
                }));
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "operator_performance.csv");
        }

        [HttpGet]
        public async Task<IActionResult> Operator(int id, int days = 0, double? maxProgramHours = null)
        {
            // Operatör temel bilgisi (isim vb.) almak için aggregate list edilip ya da context üzerinden bakmak yerine performance sonucu üzerinden elde etmeye çalışacağız.
            var programlar = await _operatorService.GetOperatorProgramDetailsAsync(id, days, maxProgramHours);
            // Bir program varsa isimleri çıkaramayabilirsek fallback olarak DB'den operator alabiliriz.
            string adSoyad = string.Empty;
            string kullaniciAdi = string.Empty;
            if (programlar.Any())
            {
                // Program detay query'si operator meta getirmiyor; context'ten çekelim.
                // (Burada doğrudan context kullanmak yerine service'e ek method düşünülebilir; şimdilik küçük sorgu.)
                // Basit yaklaşım: operator tablosu varsa çek.
                // Reflection / context erişimi yok burada - DI edilen service'te context yok. Controller içinde context yok. Alternatif: aggregate listten al.
                // Geçici çözüm: OperatorService içine küçük bir method eklemek yerine, isim bilgisini almak için performance listesine başvurabiliriz.
                // Şimdilik sadece id gösterelim; daha sonra geliştirilebilir.
            }
            var vm = new OperatorDetailVM
            {
                OperatorId = id,
                AdSoyad = adSoyad == string.Empty ? $"Operatör #{id}" : adSoyad,
                KullaniciAdi = kullaniciAdi,
                Programlar = programlar,
                Days = days,
                MaxProgramHours = maxProgramHours,
                OlusturmaZamani = DateTime.Now
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> OperatorCsv(int id, int days = 0, double? maxProgramHours = null)
        {
            var rows = await _operatorService.GetOperatorProgramDetailsAsync(id, days, maxProgramHours);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ProgramId;ProgramAdi;KaynakToplam;BasariliKaynak;BasarisizKaynak;BasariYuzdesi;HataSayisi;SureSn;BitisZamani");
            foreach (var r in rows)
            {
                var sureSn = r.ProgramSuresi?.TotalSeconds.ToString("F0") ?? "";
                sb.AppendLine(string.Join(';', new [] {
                    r.ProgramId.ToString(),
                    r.ProgramAdi.Replace(';',','),
                    r.KaynakToplam.ToString(),
                    r.BasariliKaynak.ToString(),
                    r.BasarisizKaynak.ToString(),
                    r.BasariYuzdesi.ToString(),
                    r.HataSayisi.ToString(),
                    sureSn,
                    r.BitisZamani.ToString("yyyy-MM-dd HH:mm:ss")
                }));
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"operator_{id}_programlar.csv");
        }
    }
}
