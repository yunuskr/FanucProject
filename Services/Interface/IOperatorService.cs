using System.Collections.Generic;
using System.Threading.Tasks;
using FanucRelease.ViewModels;

namespace FanucRelease.Services.Interfaces
{
    public interface IOperatorService
    {
    Task<List<OperatorPerformanceRow>> GetOperatorPerformanceAsync(int minProgram = 0, int days = 0, double? maxProgramHours = null);
    Task<List<OperatorProgramDetailRow>> GetOperatorProgramDetailsAsync(int operatorId, int days = 0, double? maxProgramHours = null);
    }
}