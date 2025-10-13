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
    public async Task<List<FanucRelease.Models.ViewModels.GecmisProgramRow>> GetGecmisProgramlarRowsAsync(int take = 100)
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

                // Fallback: kaynaklardan süre toplanmadıysa program baş/bitiş farkını kullan
                if (toplam == TimeSpan.Zero && p.BaslangicZamani > DateTime.MinValue && p.BitisZamani > DateTime.MinValue && p.BitisZamani > p.BaslangicZamani)
                {
                    toplam = p.BitisZamani - p.BaslangicZamani;
                }

                string SureToText(TimeSpan ts)
                    => ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}h {ts.Minutes}m"
                     : ts.TotalMinutes >= 1 ? $"{ts.Minutes}m"
                     : $"{ts.Seconds}s";

                // Toplam Kaynak Sayısı: öncelik ilişkili Kaynaklar; yoksa p.KaynakSayisi
                int kaynakSayisi = p.Kaynaklar?.Count ?? (p.KaynakSayisi > 0 ? p.KaynakSayisi : 0);

                // Başarı yüzdesi: başarılı / toplam
                int basarili = p.Kaynaklar?.Count(k => k.basarili_mi) ?? 0;
                int basariYuzde = kaynakSayisi > 0 ? (int)Math.Round((double)basarili * 100.0 / kaynakSayisi) : 0;

                string badgeClass = kaynakSayisi >= 25 ? "badge badge-success"
                                  : kaynakSayisi >= 15 ? "badge badge-warning"
                                  :                       "badge badge-danger";

                return new FanucRelease.Models.ViewModels.GecmisProgramRow
                {
                    Id = p.Id,
                    ProgramAdi = p.ProgramAdi ?? "—",
                    KaynakSayisi = kaynakSayisi,
                    OperatorAdSoyad = p.Operator is null ? "—" : $"{p.Operator.Ad} {p.Operator.Soyad}",
                    ToplamSureText = toplam == TimeSpan.Zero ? "—" : SureToText(toplam),
                    // Tarih: önce kaynakların max bitişi, yoksa program.Tarih ya da program.BitisZamani
                    TarihText = (maxBitis ?? (p.Tarih > DateTime.MinValue ? p.Tarih : (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : (DateTime?)null)))?.ToString("dd.MM.yyyy") ?? "—",
                    BasariYuzde = basariYuzde,
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

        public async Task<(int thisWeek, int lastWeek)> GetProgramCountsForThisAndLastWeekAsync()
        {
            // Define week as Monday 00:00 to next Monday 00:00 (local time)
            DateTime now = DateTime.Now;
            // get Monday of current week
            int diffToMonday = ((int)now.DayOfWeek + 6) % 7; // Monday=0
            DateTime thisWeekStart = now.Date.AddDays(-diffToMonday);
            DateTime thisWeekEnd = thisWeekStart.AddDays(7);

            DateTime lastWeekStart = thisWeekStart.AddDays(-7);
            DateTime lastWeekEnd = thisWeekStart; // exclusive

            // Use Tarih if available; if missing, fallback to BitisZamani or BaslangicZamani
            var query = _context.ProgramVerileri.AsNoTracking().Select(p => new
            {
                Tarih = p.Tarih > DateTime.MinValue ? p.Tarih : (p.BitisZamani > DateTime.MinValue ? p.BitisZamani : p.BaslangicZamani)
            });

            int thisWeekCount = await query.CountAsync(x => x.Tarih >= thisWeekStart && x.Tarih < thisWeekEnd);
            int lastWeekCount = await query.CountAsync(x => x.Tarih >= lastWeekStart && x.Tarih < lastWeekEnd);

            return (thisWeekCount, lastWeekCount);
        }

        /// <summary>
        /// Detay görünümü için son "take" adet programı Hatalar ile birlikte getirir.
        /// </summary>
        public async Task<List<ProgramVerisi>> GetRecentProgramsWithHatalarAsync(int take = 20)
        {
            return await _context.ProgramVerileri
                .AsNoTracking()
                .Include(p => p.Hatalar)
                .OrderByDescending(p => p.Id)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Id'ye göre Program'ı Operator, Kaynaklar ve Hatalar dahil getirir.
        /// </summary>
        public async Task<ProgramVerisi?> GetProgramWithDetailsByIdAsync(int id)
        {
            return await _context.ProgramVerileri
                .AsNoTracking()
                .Include(p => p.Operator)
                .Include(p => p.Kaynaklar)
                .Include(p => p.Hatalar)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// En güncel programı (Id'ye göre) detaylarıyla getirir.
        /// </summary>
        public async Task<ProgramVerisi?> GetLatestProgramWithDetailsAsync()
        {
            return await _context.ProgramVerileri
                .AsNoTracking()
                .Include(p => p.Operator)
                .Include(p => p.Kaynaklar)
                .Include(p => p.Hatalar)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Sadece başlık bilgileri için (Operator dahil) programı getirir.
        /// </summary>
        public async Task<ProgramVerisi?> GetProgramHeaderByIdAsync(int id)
        {
            return await _context.ProgramVerileri
                .AsNoTracking()
                .Include(p => p.Operator)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
