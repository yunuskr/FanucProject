using FanucRelease.Data;
using FanucRelease.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.ViewModels;
public class ProgramDetayController : Controller
{
    private readonly ApplicationDbContext _db;
    public ProgramDetayController(ApplicationDbContext db) => _db = db;

    // GET: /ProgramDetay/Index   veya  /ProgramDetay/Index/{id}
    [HttpGet]
    public async Task<IActionResult> Index(int? id)
    {
        // Geçmiş programlar listesi (son 10–20 kayıt)
        var sonProgramlar = await _db.ProgramVerileri
            .AsNoTracking()
            .Include(p => p.Hatalar)      // view'da hata sayısı için
            .OrderByDescending(p => p.Id) // en yeniler üstte
            .Take(20)
            .ToListAsync();

        ViewBag.SonProgramlar = sonProgramlar;

        // Detayını göstereceğimiz program
        var detayQuery = _db.ProgramVerileri
            .AsNoTracking()
            .Include(p => p.Operator)
            .Include(p => p.Kaynaklar)
            .Include(p => p.Hatalar);

        ProgramVerisi? program;

        if (id.HasValue && id.Value > 0)
            program = await detayQuery.FirstOrDefaultAsync(p => p.Id == id.Value);
        else
            program = await detayQuery.OrderByDescending(p => p.Id).FirstOrDefaultAsync();

        if (program is null)
            return NotFound("Gösterilecek program bulunamadı.");

        return View(program);
    }
    [HttpGet]
    public async Task<IActionResult> KaynakDetay(int id)
    {
        if (id <= 0) return BadRequest();

        var kaynak = await _db.Kaynaklar
            .AsNoTracking()
            .Include(k => k.ProgramVerisi)
            .Include(k => k.AnlikKaynaklar)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kaynak is null) return NotFound();

        var ornekler = kaynak.AnlikKaynaklar
            .OrderBy(x => x.OlcumZamani)
            .ToList();

        double avgV  = ornekler.Any() ? ornekler.Average(x => x.Voltaj)        : 0;
        double avgA  = ornekler.Any() ? ornekler.Average(x => x.Amper)         : 0;
        double avgTS = ornekler.Any() ? ornekler.Average(x => x.TelSurmeHizi)  : 0;
        double avgKH = ornekler.Any() ? ornekler.Average(x => x.KaynakHizi)    : 0;

        var vm = new KaynakDetayVM
        {
            Kaynak = kaynak,
            Ornekler = ornekler,
            OrtalamaVoltaj = Math.Round(avgV, 2),
            OrtalamaAmper = Math.Round(avgA, 2),
            OrtalamaTelSurme = Math.Round(avgTS, 2),
            OrtalamaKaynakHizi = Math.Round(avgKH, 2)
        };

        return View(vm); // Views/ProgramDetay/KaynakDetay.cshtml
    }
}
