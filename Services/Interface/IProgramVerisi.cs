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
    }
}
