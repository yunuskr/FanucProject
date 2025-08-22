using FanucRelease.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.Data;
namespace FanucRelease.Controllers
{
    public class AKaynakParametreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AKaynakParametreController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var vm = new KaynakViewModel
            {
                Donguler = new List<KaynakDongusu>
        {
            new KaynakDongusu { Id = 1, BaslangicSaati = new TimeOnly(10, 0), BitisSaati = new TimeOnly(10, 30), ToplamSureSaniye = 1800, Tamamlandi = true, OlusturulmaTarihi = DateTime.Today, Operator = new Operator { Ad = "Ali", Soyad = "Demir" } }
        },
                Parametreler = new List<KaynakParametre>
        {
            new KaynakParametre { Id = 1, OlcumZamani = new TimeOnly(10, 5), Voltaj = 24, Amper = 120, TelSurmeHizi = 5 },
            new KaynakParametre { Id = 2, OlcumZamani = new TimeOnly(10, 10), Voltaj = 25, Amper = 115, TelSurmeHizi = 6 },
            new KaynakParametre { Id = 3, OlcumZamani = new TimeOnly(10, 20), Voltaj = 23, Amper = 118, TelSurmeHizi = 5 }
        }
            };

            return View(vm);
        }
    [HttpGet]
    public IActionResult GetLiveData()
    {
        // Burada gerçek SQL'den veya robot API'sinden veri alacaksın
        var parametreler = new List<KaynakParametre>
        {
            new KaynakParametre { OlcumZamani = TimeOnly.FromDateTime(DateTime.Now), Voltaj = new Random().Next(20, 30), Amper = new Random().Next(100, 130), TelSurmeHizi = new Random().Next(5, 10) }
        };

        return Json(parametreler);
    }
    }
}
