using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.Models.Services;
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
    public class ChallengeController : BaseController
    {
        #region Fields

        private readonly IChallengeService challengeService;
        private readonly IUserService userService;
        private readonly ILogger<ChallengeController> logger;


        #endregion

        #region Constructors

        public ChallengeController(IChallengeService _challengeService, IUserService _userService, ILogger<ChallengeController> _logger)
        {
            challengeService = _challengeService;
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

        [HttpGet("GetProjectDetailHeader")]
        public async Task<GeneralResponse> GetProjectDetailHeader(string projectid)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await challengeService.GetProjectDetailHeader(projectid, GetCurrentUserId());
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

        [HttpGet("GetChallengeDetails")]
        public async Task<GeneralResponse> GetChallengeDetails(string projectId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await challengeService.GetChallengeDetails(projectId, GetCurrentUserId());
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

        [HttpPost("AddListToChallenge")]
        public async Task<GeneralResponse> AddListToChallenge(int? challengeId, string listName)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await challengeService.AddListToChallenege(challengeId, listName, GetCurrentUserId());
                var data = await challengeService.GetUpdatedList(result);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = data;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("AddCardToList")]
        public async Task<GeneralResponse> AddCardToList(int? listId, string cardName)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await challengeService.AddCardToList(listId, cardName, GetCurrentUserId());
                var data = await challengeService.GetUpdatedCard(result);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = data;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("DeleteList")]
        public async Task<GeneralResponse> DeleteList(int? listId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await challengeService.DeleteList(listId, GetCurrentUserId());
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

        [HttpGet("AddDuration")]
        public GeneralResponse AddDuration(double mins, int challengeid)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var updatedTime = challengeService.AddDuration(mins, challengeid, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
                response.Data = updatedTime;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetUpdatedChallengeLists")]
        public async Task<GeneralResponse> GetUpdatedChallengeLists(int challengeId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await challengeService.GetUpdatedChallengeLists(challengeId);
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
        
        [HttpPut("UpdateListName")]
        public GeneralResponse UpdateListName(int listId, string listName)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                challengeService.UpdateListName(listId, listName, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }
        
        [HttpPut("MarkChallengeComplete")]
        public async Task<GeneralResponse> MarkChallengeComplete(int challengeId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await challengeService.MarkChallengeComplete(challengeId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPut("MarkChallengeInComplete")]
        public async Task<GeneralResponse> MarkChallengeInComplete(int challengeId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await challengeService.MarkChallengeInComplete(challengeId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
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
