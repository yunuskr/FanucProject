using System.Threading.Tasks;

namespace FanucRelease.Services.Interfaces
{
	public interface IHataService
	{
		// Global toplam hata sayısı
		Task<int> GetToplamHataCountAsync();

		// ProgramDetay/Hatalar için
		Task<List<FanucRelease.Models.Hata>> GetHatalarByProgramIdAsync(int programVerisiId);
	}
}

