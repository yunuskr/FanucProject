using FanucRelease.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.ViewModels;
using FanucRelease.Services.Interfaces;
public class ProgramDetayController : Controller
{
    private readonly IProgramVerisiService _programService;
    private readonly IKaynakService _kaynakService;
    private readonly IHataService _hataService;

    public ProgramDetayController(
        IProgramVerisiService programService,
        IKaynakService kaynakService,
        IHataService hataService)
    {
        _programService = programService;
        _kaynakService = kaynakService;
        _hataService = hataService;
    }

    // GET: /ProgramDetay/Index   veya  /ProgramDetay/Index/{id}
    [HttpGet]
    public async Task<IActionResult> Index(int? id)
    {
        // Geçmiş programlar listesi (son 10–20 kayıt)
        var sonProgramlar = await _programService.GetRecentProgramsWithHatalarAsync(20);

        // Detayını göstereceğimiz program
        ProgramVerisi? program = null;
        if (id.HasValue && id.Value > 0)
            program = await _programService.GetProgramWithDetailsByIdAsync(id.Value);
        else
            program = await _programService.GetLatestProgramWithDetailsAsync();

        if (program is null)
            return NotFound("Gösterilecek program bulunamadı.");

        var vm = new ProgramDetayIndexVM
        {
            Program = program,
            SonProgramlar = sonProgramlar
        };

        return View(vm);
    }
    [HttpGet]
    public async Task<IActionResult> KaynakDetay(int id)
    {
        if (id <= 0) return BadRequest();

        var kaynak = await _kaynakService.GetKaynakWithDetailsByIdAsync(id);

        if (kaynak is null) return NotFound();

        var ornekler = kaynak.AnlikKaynaklar
            .OrderBy(x => x.OlcumZamani)
            .ToList();

        double avgV = ornekler.Any() ? ornekler.Average(x => x.Voltaj) : 0;
        double avgA = ornekler.Any() ? ornekler.Average(x => x.Amper) : 0;
        double avgTS = ornekler.Any() ? ornekler.Average(x => x.TelSurmeHizi) : 0;
        double avgKH = ornekler.Any() ? ornekler.Average(x => x.KaynakHizi) : 0;

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
    [HttpGet]
public async Task<IActionResult> Hatalar(int id)
{
    if (id <= 0) return BadRequest();

    // Program bilgisi (başlık/geri dönüş için)
    var program = await _programService.GetProgramHeaderByIdAsync(id);

    if (program is null) return NotFound();

    // Bu programa ait tüm hatalar
    var hatalar = await _hataService.GetHatalarByProgramIdAsync(id);

    var vm = new ProgramHatalarVM
    {
        ProgramId = program.Id,
        ProgramAdi = program.ProgramAdi,
        OperatorAdSoyad = program.Operator is null ? null : $"{program.Operator.Ad} {program.Operator.Soyad}",
        Hatalar = hatalar.Select(h => new ProgramHatalarVM.HataRow
        {
            Id = h.Id,
            Tip = h.Tip.ToString(),
            Aciklama = h.Aciklama,
            Zaman = h.Zaman
        }).ToList()
    };

    return View(vm); // Views/ProgramDetay/Hatalar.cshtml
}
}
