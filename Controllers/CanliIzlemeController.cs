// Controllers/CanliIzlemeController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.Data;
using FanucRelease.Services.Interfaces;
using FanucRelease.Models;

namespace FanucRelease.Controllers
{
    [Route("[controller]")]
    public class CanliIzlemeController : Controller
    {
    private readonly IAnlikKaynakService _anlikService;
    public CanliIzlemeController(IAnlikKaynakService anlikService) => _anlikService = anlikService;

        // UI
        [HttpGet("")]
        public IActionResult Index(int? kaynakId = null, int? programId = null)
        {
            ViewBag.KaynakId = kaynakId; // filtrelemek isterseniz
            ViewBag.ProgramId = programId; // opsiyonel: URL'den gelen program id
            return View();
        }

        // JSON: Son N kayıt (grafikler ve durum için)
        // /CanliIzleme/api/son?count=120&kaynakId=5
        [HttpGet("api/son")]
        public async Task<IActionResult> Son(int count = 120, int? kaynakId = null)
        {
            var list = await _anlikService.GetRecentAsync(count, kaynakId);
            var items = list
                .Select(x => new
                {
                    t = x.OlcumZamani,
                    v = x.Voltaj,
                    a = x.Amper,
                    w = x.TelSurmeHizi,
                    hiz = x.KaynakHizi
                })
                .ToList();

            // Service returns newest-first; reverse for ascending order in charts
            items.Reverse();

            // ---- Basit durum çıkarımı ----
            // "Çalışıyor mu?" mantığı: son 10 sn içinde veri + akım > 5A veya hız > 0
            var now = DateTime.UtcNow;
            var last = items.LastOrDefault();
            bool veriTaze = last != null && (now - last.t.ToUniversalTime()).TotalSeconds <= 10;
            bool akimVar = last != null && last.a > 5;
            bool hizVar = last != null && last.hiz > 0;
            bool robotCalisiyor = veriTaze && (akimVar || hizVar);

            // Alarm var mı? (örnek mantık: akım 0 ve hız > 0 veya Voltaj aşırı)
            bool alarmVar = false;
            string? alarmMesaji = null;
            if (last != null)
            {
                if (last.hiz > 0 && last.a <= 0.5)
                {
                    alarmVar = true;
                    alarmMesaji = "Tel sürme var fakat akım yok";
                }
                else if (last.v > 45) // eşik örnek
                {
                    alarmVar = true;
                    alarmMesaji = "Yüksek voltaj";
                }
            }

            var status = new
            {
                robotCalisiyor,
                robotHizi = last?.hiz ?? 0,
                alarmVar,
                alarmMesaji
            };

            return Json(new { status, items });
        }
    }
}
