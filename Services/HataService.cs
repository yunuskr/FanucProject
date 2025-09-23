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

