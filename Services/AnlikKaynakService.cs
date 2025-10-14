using FanucRelease.Data;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Services
{
    public class AnlikKaynakService : IAnlikKaynakService
    {
        private readonly ApplicationDbContext _db;
        public AnlikKaynakService(ApplicationDbContext db) => _db = db;

        public async Task<List<AnlikKaynak>> GetRecentAsync(int count, int? kaynakId = null)
        {
            var q = _db.AnlikKaynaklar.AsNoTracking();
            if (kaynakId.HasValue)
                q = q.Where(x => x.KaynakId == kaynakId.Value);

            count = Math.Clamp(count, 10, 1000);
            var list = await q
                .OrderByDescending(x => x.OlcumZamani)
                .Take(count)
                .ToListAsync();

            return list;
        }
    }
}
