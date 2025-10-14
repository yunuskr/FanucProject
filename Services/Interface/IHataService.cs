using System.Threading.Tasks;

namespace FanucRelease.Services.Interfaces
{
	public interface IHataService
	{
		// Global toplam hata sayısı
		Task<int> GetToplamHataCountAsync();

		// Haftalık hata sayıları (bu hafta vs geçen hafta)
		Task<(int thisWeek, int lastWeek)> GetHataCountsForThisAndLastWeekAsync();

		// ProgramDetay/Hatalar için
		Task<List<FanucRelease.Models.Hata>> GetHatalarByProgramIdAsync(int programVerisiId);
	}
}

