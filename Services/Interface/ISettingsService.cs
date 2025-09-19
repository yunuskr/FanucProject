using FanucRelease.Models;
using System.Threading.Tasks;

namespace FanucRelease.Services.Interfaces
{
    public interface ISettingsService
    {
        Task SaveOrUpdateAsync(Setting model);
        string? GetDynamicConnection();
        Task<string> TestDynamicDbAsync();
    }
}
