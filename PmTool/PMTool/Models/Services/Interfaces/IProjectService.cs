using DAL.Models;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using PMTool.Resources.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Services
{
    public interface IProjectService
    {
        Task<bool> CreateProject(CreateProjectReq createProjectReq, int? currentUserId);
        Task<bool> UpdateProject(CreateProjectReq projectReq, int? currentUserId);
        Task<ProjectListDtoPage> GetProjects(GetAllPageReq getProjectsReq, int? currentUserid);
        Task DeleteProject(string id, int? currentUserId);
        Task<EditProjectDto> GetProjectById(string id, int? currentUserId);
        Task<List<ProjectParticipants>> GetProjectMembers(int projectId);
        Project GetProjectByPublicId(string id);
        Task<List<ProjectMemberDto>> AddMembersByInvite(List<string> Emails, string projectId, int? currentUserId);
        List<MemberDDL> GetExistingProjectMembers(string id);
        Task<List<GeneralDto>> GetAllUsers(string projectId);
        Task<List<ProjectPermissionDto>> GetAllPermissions();
        List<ProjectMemberPermissionDto> GetAllUserPermissions(string projectId);
        void UpdateProjectPermissions(string projectId, List<UpdateProjectPermissionRequest> updateProjectPermissionRequest, int? currentUserId);
        Task<List<ProjectPermissionAllowedDto>> GetProjectMemberPermissions(string projectId, int? currentUserId);
    }
}