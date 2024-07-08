using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using PMTool.Models.Response;
using PMTool.Resources.Response;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Controllers
{
    [Authorize(Policy = "ActiveUserPolicy")]
    public class TeamController : BaseController
    {
        #region Fields

        private readonly ITeamService teamService;
        private readonly IUserService userService;
        private readonly ILogger<TeamController> logger;

        #endregion

        #region Constructors

        public TeamController(ITeamService _teamService, IUserService _userService, ILogger<TeamController> _logger)
        {
            teamService = _teamService;
            userService = _userService;
            logger = _logger;
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

        [HttpPost("UpsertTeam")]
        public async Task<GeneralResponse> UpsertTeam(UpsertTeamReq upsertTeamReq)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                response = await teamService.UpsertTeam(upsertTeamReq, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
                if (upsertTeamReq.TeamId == null)
                    response.Message = "Team Created Successfully";
                else
                    response.Message = "Team Updated Successfully";
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("GetAllTeams")]
        public async Task<GeneralResponse> GetAllTeams(GetAllPageReq getAllTeamsReq)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var teams = await teamService.GetAllTeams(getAllTeamsReq, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = teams;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetTeamById")]
        public async Task<GeneralResponse> GetTeamById(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var res = await teamService.GetTeamById(id, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteTeam")]
        public async Task<GeneralResponse> DeleteTeam(string teamId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await teamService.DeleteTeam(teamId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);
                response.Message = "Team Deleted Successfully";
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
