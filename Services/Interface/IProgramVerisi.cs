using FanucRelease.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FanucRelease.Services.Interfaces
{
    public interface IProgramVerisiService:IGenericService<ProgramVerisi>
    {
        Task<List<FanucRelease.Models.ViewModels.GecmisProgramRow>> GetGecmisProgramlarRowsAsync(int take = 100);
        Task<ProgramVerisi?> GetLastProgramAsync();
        Task<int> GetTotalProgramCountAsync();
        // ProgramDetay sayfası için eklenen yardımcılar
        Task<List<ProgramVerisi>> GetRecentProgramsWithHatalarAsync(int take = 20);
        Task<ProgramVerisi?> GetProgramWithDetailsByIdAsync(int id);
        Task<ProgramVerisi?> GetLatestProgramWithDetailsAsync();
        Task<ProgramVerisi?> GetProgramHeaderByIdAsync(int id);
    }
}
