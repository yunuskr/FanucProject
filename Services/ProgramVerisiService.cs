using FanucRelease.Data;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Services
{
    public class ProgramVerisiService : GenericService<ProgramVerisi>, IProgramVerisiService
    {
        private readonly ApplicationDbContext _context;

        public ProgramVerisiService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Home/Index ve gecmis listesi için satırlar
        public async Task<List<object>> GetGecmisProgramlarRowsAsync(int take = 100)
        {
            var programlar = await _context.ProgramVerileri
                .AsNoTracking()
                .Include(p => p.Operator)
                .Include(p => p.Kaynaklar)
                .OrderByDescending(p => p.Id)
                .Take(take)
                .ToListAsync();

            var rows = programlar.Select(p =>
            {
                TimeSpan toplam = TimeSpan.Zero;
                DateTime? maxBitis = null;

                foreach (var k in p.Kaynaklar)
                {
                    var d = k.ToplamSure != default
                            ? k.ToplamSure.ToTimeSpan()
                            : (k.BitisSaati - k.BaslangicSaati);

                    if (d > TimeSpan.Zero) toplam += d;
                    if (maxBitis is null || k.BitisSaati > maxBitis.Value)
                        maxBitis = k.BitisSaati;
                }

                string SureToText(TimeSpan ts)
                    => ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}h {ts.Minutes}m"
                     : ts.TotalMinutes >= 1 ? $"{ts.Minutes}m"
                     : $"{ts.Seconds}s";

                int kaynakSayisi = p.KaynakSayisi > 0 ? p.KaynakSayisi : p.Kaynaklar.Count;
                string badgeClass = kaynakSayisi >= 25 ? "badge badge-success"
                                  : kaynakSayisi >= 15 ? "badge badge-warning"
                                  :                       "badge badge-danger";

                return (object)new
                {
                    p.Id,
                    p.ProgramAdi,
                    KaynakSayisi = kaynakSayisi,
                    OperatorAdSoyad = p.Operator is null ? "—" : $"{p.Operator.Ad} {p.Operator.Soyad}",
                    ToplamSureText = toplam == TimeSpan.Zero ? "—" : SureToText(toplam),
                    TarihText = maxBitis?.ToString("dd.MM.yyyy") ?? "—",
                    BasariYuzde = 100,
                    BadgeClass = badgeClass
                };
            }).ToList();

            return rows;
        }

        // En son program: 'Tarih' alanına göre en güncel kayıt
        public async Task<ProgramVerisi?> GetLastProgramAsync()
        {
            // 1) Sadece Id çekerek en güncel programı bul (Tarih'e göre; eşitlikte Id'ye göre)
            var lastId = await _context.ProgramVerileri
                .AsNoTracking()
                .OrderByDescending(p => p.Tarih)
                .ThenByDescending(p => p.Id)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (lastId == 0)
                return null;

            // 2) İlişkileriyle birlikte o Id'li programı getir
            var program_verisi = await _context.ProgramVerileri
                .AsNoTracking()
                .Include(p => p.Operator)
                .Include(p => p.Kaynaklar)
                .Include(p => p.Hatalar)
                .FirstOrDefaultAsync(p => p.Id == lastId);

            return program_verisi;
        }

        public async Task<int> GetTotalProgramCountAsync()
        {
            return await _context.ProgramVerileri.AsNoTracking().CountAsync();
        }
    }
}
