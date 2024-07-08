using PMTool.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public interface IReportService
    {
        Task<Dictionary<string, int>> GetPerformanceMatrix(int? currentUserId);
        Task<List<ProjectReportDto>> GetProjectReport(int? currentUserId);
        Task<Dictionary<string, object>> GetProjectWiseReport(string projectId);
        Task<List<GeneralDto>> GetUserProjects(int? currentUserId);
    }
}