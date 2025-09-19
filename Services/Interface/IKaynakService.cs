using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.Services.Interfaces
{
    public interface IKaynakService
    {
        Task<int> GetToplamKaynakCountAsync(int programVerisiId);
        Task<int> GetBasariliKaynakCountAsync(int programVerisiId);
        Task<(int toplam, int basarili)> GetKaynakCountsAsync(int programVerisiId);

        // Global (tüm kayıtlar) sayımlar
        Task<int> GetToplamKaynakCountAsync();
        Task<int> GetBasariliKaynakCountAsync();
        Task<(int toplam, int basarili)> GetKaynakCountsAsync();
        Task<TimeSpan> GetBugunToplamSureAsync();
    }
}
