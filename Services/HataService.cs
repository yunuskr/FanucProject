using FanucRelease.Data;
using FanucRelease.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FanucRelease.Services
{
	public class HataService : IHataService
	{
		private readonly ApplicationDbContext _db;
		public HataService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<int> GetToplamHataCountAsync()
		{
			return await _db.Hatalar.AsNoTracking().CountAsync();
		}

		public async Task<(int thisWeek, int lastWeek)> GetHataCountsForThisAndLastWeekAsync()
		{
			DateTime now = DateTime.Now;
			int diffToMonday = ((int)now.DayOfWeek + 6) % 7; // Monday=0
			DateTime thisWeekStart = now.Date.AddDays(-diffToMonday);
			DateTime thisWeekEnd = thisWeekStart.AddDays(7);
			DateTime lastWeekStart = thisWeekStart.AddDays(-7);
			DateTime lastWeekEnd = thisWeekStart;

			var q = _db.Hatalar.AsNoTracking().Select(h => h.Zaman);
			int thisWeekCount = await q.CountAsync(t => t >= thisWeekStart && t < thisWeekEnd);
			int lastWeekCount = await q.CountAsync(t => t >= lastWeekStart && t < lastWeekEnd);
			return (thisWeekCount, lastWeekCount);
		}

		public async Task<List<FanucRelease.Models.Hata>> GetHatalarByProgramIdAsync(int programVerisiId)
		{
			return await _db.Hatalar
				.AsNoTracking()
				.Where(h => h.ProgramVerisiId == programVerisiId)
				.OrderByDescending(h => h.Zaman)
				.ToListAsync();
		}
	}
}

