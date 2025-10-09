using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using FanucRelease.ViewModels;

namespace FanucRelease.Services
{
    public class OperatorService : IOperatorService
    {
        private readonly ApplicationDbContext _context;

        public OperatorService(ApplicationDbContext context)
        {
            _context = context;
        }

      
        /// <summary>
        /// Operatör bazında Program, Kaynak, Başarı, Hata ve süre metriklerini toplar.
        /// </summary>
        public async Task<List<OperatorPerformanceRow>> GetOperatorPerformanceAsync(int minProgram = 0, int days = 0, double? maxProgramHours = null)
        {
            var query = _context.ProgramVerileri.AsNoTracking().Where(p => p.OperatorId != null);
            if (days > 0)
            {
                var threshold = DateTime.Now.AddDays(-days);
                query = query.Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= threshold);
            }

            // İlk faz: program bazlı düz projeksiyon (navigasyon koleksiyonlarını sayısal alt sorgularla hesapla)
            var programFlat = await query
                .Select(p => new
                {
                    p.Id,
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

            // İkinci faz: memory’de manuel grupla – Operatör başına TEK satır garanti.
            var dict = new Dictionary<int, (string Ad,string Soyad,string Kullanici,
                                           int ProgramSayisi,int ToplamKaynak,int BasariliKaynak,int HataSayisi,DateTime? SonProgram, long ToplamSureTicks,int SureAdet)>();
            foreach (var p in programFlat)
            {
                if (!p.OperatorId.HasValue) continue;
                var id = p.OperatorId.Value;
                if (!dict.TryGetValue(id, out var acc))
                {
                    acc = (p.OpAd ?? string.Empty, p.OpSoyad ?? string.Empty, p.OpUser ?? string.Empty,
                           0,0,0,0,null,0L,0);
                }
                acc.ProgramSayisi += 1;
                acc.ToplamKaynak += p.KaynakToplam;
                acc.BasariliKaynak += p.KaynakBasarili;
                acc.HataSayisi += p.HataSayisi;
                if (acc.SonProgram == null || p.BitisOrFallback > acc.SonProgram)
                    acc.SonProgram = p.BitisOrFallback;
                if (p.SureTicks.HasValue)
                {
                    acc.ToplamSureTicks += p.SureTicks.Value;
                    acc.SureAdet += 1;
                }
                dict[id] = acc;
            }
            var grouped = dict.Select(kv => new {
                OperatorId = (int?)kv.Key,
                OpAd = kv.Value.Ad,
                OpSoyad = kv.Value.Soyad,
                OpUser = kv.Value.Kullanici,
                ProgramSayisi = kv.Value.ProgramSayisi,
                ToplamKaynak = kv.Value.ToplamKaynak,
                BasariliKaynak = kv.Value.BasariliKaynak,
                HataSayisi = kv.Value.HataSayisi,
                SonProgramZamani = kv.Value.SonProgram,
                OrtalamaSureTicks = kv.Value.SureAdet > 0 ? (double)kv.Value.ToplamSureTicks / kv.Value.SureAdet : 0d
            }).ToList();

            if (maxProgramHours.HasValue && maxProgramHours.Value > 0)
            {
                var maxTicks = TimeSpan.FromHours(maxProgramHours.Value).Ticks;
                grouped = grouped.Where(g => g.OrtalamaSureTicks <= maxTicks).ToList();
            }

            var rows = grouped
                .Where(x => x.ProgramSayisi >= minProgram)
                .Select(x => new OperatorPerformanceRow
                {
                    OperatorId = x.OperatorId ?? 0,
                    KullaniciAdi = x.OpUser ?? string.Empty,
                    AdSoyad = ($"{x.OpAd} {x.OpSoyad}").Trim(),
                    ProgramSayisi = x.ProgramSayisi,
                    ToplamKaynak = x.ToplamKaynak,
                    BasariliKaynak = x.BasariliKaynak,
                    HataSayisi = x.HataSayisi,
                    SonProgramZamani = x.SonProgramZamani,
                    OrtalamaProgramSuresi = x.OrtalamaSureTicks > 0 ? TimeSpan.FromTicks((long)x.OrtalamaSureTicks) : null
                })
                .OrderByDescending(r => r.ProgramSayisi)
                .ThenByDescending(r => r.BasariYuzdesi)
                .ToList();
            return rows;
        }

        /// <summary>
        /// Belirli bir operatöre ait program detaylarını (program bazlı) döndürür.
        /// </summary>
        public async Task<List<OperatorProgramDetailRow>> GetOperatorProgramDetailsAsync(int operatorId, int days = 0, double? maxProgramHours = null)
        {
            var query = _context.ProgramVerileri.AsNoTracking()
                .Where(p => p.OperatorId == operatorId);
            if (days > 0)
            {
                var threshold = DateTime.Now.AddDays(-days);
                query = query.Where(p => (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih) >= threshold);
            }

            var flat = await query
                .Select(p => new
                {
                    p.Id,
                    p.ProgramAdi,
                    KaynakToplam = p.Kaynaklar.Count,
                    Basarili = p.Kaynaklar.Count(k => k.basarili_mi),
                    HataSayisi = p.Hatalar.Count,
                    BitisOrFallback = p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.Tarih,
                    SureTicks = (p.BitisZamani > DateTime.MinValue && p.BaslangicZamani > DateTime.MinValue && p.BitisZamani > p.BaslangicZamani)
                        ? (long?)(p.BitisZamani - p.BaslangicZamani).Ticks : null
                })
                .ToListAsync();

            if (maxProgramHours.HasValue && maxProgramHours.Value > 0)
            {
                var maxTicks = TimeSpan.FromHours(maxProgramHours.Value).Ticks;
                flat = flat.Where(f => !f.SureTicks.HasValue || f.SureTicks.Value <= maxTicks).ToList();
            }

            var rows = flat
                .Select(f => new OperatorProgramDetailRow
                {
                    ProgramId = f.Id,
                    ProgramAdi = f.ProgramAdi,
                    KaynakToplam = f.KaynakToplam,
                    BasariliKaynak = f.Basarili,
                    HataSayisi = f.HataSayisi,
                    BitisZamani = f.BitisOrFallback,
                    ProgramSuresi = f.SureTicks.HasValue ? TimeSpan.FromTicks(f.SureTicks.Value) : null
                })
                .OrderByDescending(r => r.BitisZamani)
                .ThenByDescending(r => r.KaynakToplam)
                .ToList();
            return rows;
        }
    }
}