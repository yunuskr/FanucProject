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
	}
}

