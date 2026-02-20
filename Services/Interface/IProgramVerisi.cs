using FanucRelease.Models;


namespace FanucRelease.Services.Interfaces
{
    public interface IProgramVerisiService:IGenericService<ProgramVerisi>
    {
        Task<List<Models.ViewModels.GecmisProgramRow>> GetGecmisProgramlarRowsAsync(int take = 100);
        Task<ProgramVerisi?> GetLastProgramAsync();
        Task<int> GetTotalProgramCountAsync();
        Task<(int thisWeek, int lastWeek)> GetProgramCountsForThisAndLastWeekAsync();
        // ProgramDetay sayfası için eklenen yardımcılar
        Task<List<ProgramVerisi>> GetRecentProgramsWithHatalarAsync(int take = 20);
        Task<ProgramVerisi?> GetProgramWithDetailsByIdAsync(int id);
        Task<ProgramVerisi?> GetLatestProgramWithDetailsAsync();
        Task<ProgramVerisi?> GetProgramHeaderByIdAsync(int id);
    }
}
