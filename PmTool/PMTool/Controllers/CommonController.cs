using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.Models.Request;
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
    public class CommonController : BaseController
    {
        #region Fields

        private readonly ICommonService commonService;
        private readonly IUserService userService;
        private readonly ILogger<CommonController> logger;

        #endregion

        #region Constructors

        public CommonController(ICommonService _commonService, IUserService _userService, ILogger<CommonController> _logger)
        {
            commonService = _commonService;
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

        [HttpGet("GetMemberDDL")]
        public GeneralResponse GetMemberDDL(string query = null)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var res = commonService.GetMemberDDL(query, GetCurrentUserId());
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

        [HttpGet("GetTeamDDL")]
        public async Task<GeneralResponse> GetTeamDDL()
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var res = await commonService.GetTeamDDL(GetCurrentUserId());
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

        [HttpPost("AddActivityLog")]
        public async Task<GeneralResponse> AddActivityLog(AddActivityLogRequest addActivityLogRequest)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await commonService.AddActivityLog(addActivityLogRequest, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
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
