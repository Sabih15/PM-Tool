using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.General;
using PMTool.Models.General;
using PMTool.Models.Request;
using PMTool.Resources.Models;
using PMTool.Resources.Request;
using PMTool.Resources.Response;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Controllers
{
    [Authorize(Policy = "ActiveUserPolicy")]
    public class ProjectController : BaseController
    {
        #region Fields

        private readonly IRepository<Project> projectRepository;
        private readonly IMapper mapper;
        private readonly IProjectService projectService;
        private readonly IUserService userService;
        private readonly ILogger<ProjectController> logger;

        #endregion

        #region Constructors

        public ProjectController(IRepository<Project> _projectRepository, IMapper _mapper,
            IProjectService _projectService,
            IUserService _userService,
            ILogger<ProjectController> _logger)
        {
            projectRepository = _projectRepository;
            projectService = _projectService;
            userService = _userService;
            logger = _logger;
            mapper = _mapper;
        }

        #endregion

        #region Methods

        #region Private

        private int? GetCurrentUserId()
        {
            string currentUserId = JwtClaimAccessor.GetClaimByType(ClaimTypes.NameIdentifier, HttpContext);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var user = userService.GetUserByPublicId(currentUserId);
                return user.UserId;
            }
            return null;
        }

        #endregion

        #region Public

        [HttpPost("GetProjects")]
        public async Task<GeneralResponse> GetProjects(GetAllPageReq getProjectsReq)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await projectService.GetProjects(getProjectsReq, GetCurrentUserId());
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("CreateProject")]
        public async Task<GeneralResponse> CreateProject(CreateProjectReq createProjectDto)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                createProjectDto.DueDate = createProjectDto.DueDate.Value.Date.AddDays(1);
                var res = await projectService.CreateProject(createProjectDto, GetCurrentUserId());
                if (res)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.CreateSuccess);
                    response.Message = Constants.PROJECT_CREATE_SUCCESS;
                }
                else
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.ProjectExist);
                    response.Message = Constants.PROJECT_CREATE_ERROR;
                }
                response.Data = res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteProject")]
        public async Task<GeneralResponse> DeleteProject(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await projectService.DeleteProject(id, GetCurrentUserId());
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DeleteSuccess);
                response.Message = "Project Deleted Successfully";
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetExistingProjectMembers")]
        public GeneralResponse GetExistingProjectMembers(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var data = projectService.GetExistingProjectMembers(id);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = data;
            }
            catch (Exception ex)
            {
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }
            return response;
        }

        [HttpGet("GetProjectById")]
        public async Task<GeneralResponse> GetProjectById(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var data = await projectService.GetProjectById(id, GetCurrentUserId());
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = data;
            }
            catch (Exception ex)
            {
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }
            return response;
        }

        [HttpPut("UpdateProject")]
        public async Task<GeneralResponse> UpdateProject(CreateProjectReq createProjectDto)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                createProjectDto.DueDate = createProjectDto.DueDate.Value.Date.AddDays(1);
                var res = await projectService.UpdateProject(createProjectDto, GetCurrentUserId());
                if (res)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.UpdateSuccess);
                    response.Message = Constants.PROJECT_UPDATE_SUCCESS;
                }
                else
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.ProjectExist);
                    response.Message = Constants.PROJECT_UPDATE_ERROR;
                }
                response.Data = res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("GetProjectMembers")]
        public async Task<GeneralResponse> GetProjectMembers(string projectId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var project = projectService.GetProjectByPublicId(projectId);
                var result = await projectService.GetProjectMembers(project.ProjectId);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("AddMembersByInvite")]
        public async Task<GeneralResponse> AddMembersByInvite(string projectId, [FromBody] List<string> Emails)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await projectService.AddMembersByInvite(Emails, projectId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetAllUsers")]
        public async Task<GeneralResponse> GetAllUsers(string projectId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await projectService.GetAllUsers(projectId);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetAllPermissions")]
        public async Task<GeneralResponse> GetAllPermissions()
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await projectService.GetAllPermissions();
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetAllMemberPermissions")]
        public GeneralResponse GetAllMemberPermissions(string projectId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = projectService.GetAllUserPermissions(projectId);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("UpdateProjectPermissions")]
        public GeneralResponse UpdateProjectPermissions(string projectId, List<UpdateProjectPermissionRequest> updateProjectPermissionRequest)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                this.projectService.UpdateProjectPermissions(projectId, updateProjectPermissionRequest, GetCurrentUserId());
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.UpdateSuccess);
                response.Message = Constants.PROJECT_PERMISSIONS_UPDATE;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetProjectMemberPermissions")]
        public async Task<GeneralResponse> GetProjectMemberPermissions(string projectId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await projectService.GetProjectMemberPermissions(projectId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        #endregion

        #endregion
    }
}
