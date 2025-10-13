using FanucRelease.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FanucRelease.Services.Interfaces
{
    public interface IAnlikKaynakService
    {
        /// Gets the most recent AnlikKaynak rows, optionally filtered by KaynakId.
        /// Returned list is ordered descending by OlcumZamani (newest first).
        Task<List<AnlikKaynak>> GetRecentAsync(int count, int? kaynakId = null);
    }
}
