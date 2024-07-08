using PMTool.Models.DTOs;
using PMTool.Models.Request;
using PMTool.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public interface ICommonService
    {
        List<MemberDDL> GetMemberDDL(string query, int? currentUserId);
        Task<List<TeamDDL>> GetTeamDDL(int? userId);
        Task AddActivityLog(AddActivityLogRequest request, int? currentUserId);
    }
}