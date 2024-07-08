using PMTool.Models.DTOs;
using PMTool.Models.Request;
using PMTool.Models.Response;
using PMTool.Resources.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Services
{
    public interface ITeamService
    {
        Task<GeneralResponse> UpsertTeam(UpsertTeamReq upsertTeamReq, int? currentUserId);
        Task<TeamListDtoPage> GetAllTeams(GetAllPageReq getAllTeamsReq, int? currentUserId);

        Task<UpsertTeamReq> GetTeamById(string id, int? currentUserId);

        List<MemberDDL> GetAllMembers(int? currentUserId);

        Task DeleteTeam(string id, int? currentUserId);
        int GetUserTeamIdByName(string name, int? userId);
        Task<bool> SendInviteEmail(string email, string inviteFor, string inviteLink);
    }
}