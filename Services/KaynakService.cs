using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using FanucRelease.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Services
{
    public class KaynakService : IKaynakService
    {
        private readonly ApplicationDbContext _context;

        public KaynakService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetToplamKaynakCountAsync(int programVerisiId)
        {
            return await _context.Kaynaklar
                .AsNoTracking()
                .CountAsync(k => EF.Property<int>(k, "ProgramVerisiId") == programVerisiId);
        }

        public async Task<int> GetBasariliKaynakCountAsync(int programVerisiId)
        {
            return await _context.Kaynaklar
                .AsNoTracking()
                .CountAsync(k => k.basarili_mi && EF.Property<int>(k, "ProgramVerisiId") == programVerisiId);
        }

        public async Task<(int toplam, int basarili)> GetKaynakCountsAsync(int programVerisiId)
        {
            var query = _context.Kaynaklar.AsNoTracking().Where(k => EF.Property<int>(k, "ProgramVerisiId") == programVerisiId);
            var toplam = await query.CountAsync();
            var basarili = await query.CountAsync(k => k.basarili_mi);
            return (toplam, basarili);
        }

        public async Task<int> GetToplamKaynakCountAsync()
        {
            return await _context.Kaynaklar.AsNoTracking().CountAsync();
        }

        public async Task<int> GetBasariliKaynakCountAsync()
        {
            return await _context.Kaynaklar.AsNoTracking().CountAsync(k => k.basarili_mi);
        }

        public async Task<(int toplam, int basarili)> GetKaynakCountsAsync()
        {
            var toplam = await _context.Kaynaklar.AsNoTracking().CountAsync();
            var basarili = await _context.Kaynaklar.AsNoTracking().CountAsync(k => k.basarili_mi);
            return (toplam, basarili);
        }

        public async Task<TimeSpan> GetBugunToplamSureAsync()
        {
            var today = DateTime.Today;
            // Bugün başlayan (aynı gün) ve başarılı olan kaynakların ToplamSure değerlerini topla
            var sureler = await _context.Kaynaklar
                .AsNoTracking()
                .Where(k => k.basarili_mi && EF.Functions.DateDiffDay(today, k.BaslangicSaati) == 0)
                .Select(k => k.ToplamSure)
                .ToListAsync();

            TimeSpan total = TimeSpan.Zero;
            foreach (var t in sureler)
            {
                total += t.ToTimeSpan();
            }
            return total;
        }

        public async Task<Kaynak?> GetLastSuccessfulKaynakOfLatestProgramAsync()
        {
            // Son programın Id'sini Tarih'e göre (eşitlikte Id) belirle
            var lastProgramId = await _context.ProgramVerileri
                .AsNoTracking()
                .OrderByDescending(p => p.Tarih)
                .ThenByDescending(p => p.Id)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (lastProgramId == 0)
                return null;

            // Bu programa ait, başarılı kaynaklar arasından en son biteni/ekleneni getir
            // Öncelik: BitisSaati varsa ona göre, yoksa Id'ye göre sırala
            var q = _context.Kaynaklar.AsNoTracking()
                .Where(k => k.basarili_mi && EF.Property<int>(k, "ProgramVerisiId") == lastProgramId);

            var last = await q
                .OrderByDescending(k => k.BitisSaati == default ? DateTime.MinValue : k.BitisSaati)
                .ThenByDescending(k => k.Id)
                .FirstOrDefaultAsync();

            return last;
        }


    }
}
