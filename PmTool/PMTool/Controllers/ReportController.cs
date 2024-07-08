using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.General;
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
    public class ReportController : BaseController
    {
        #region Fields

        private readonly IReportService reportService;
        private readonly IUserService userService;
        private readonly ILogger<ReportController> logger;

        #endregion

        #region Constructors

        public ReportController(IReportService _reportService, IUserService _userService, ILogger<ReportController> _logger)
        {
            reportService = _reportService;
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

        [HttpGet("GetUserProjects")]
        public async Task<GeneralResponse> GetUserProjects()
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await reportService.GetUserProjects(GetCurrentUserId());
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

        [HttpGet("GetReportMatrix")]
        public async Task<GeneralResponse> GetReportMatrix()
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await reportService.GetPerformanceMatrix(GetCurrentUserId());
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

        [HttpGet("GetProjectReport")]
        public async Task<GeneralResponse> GetProjectReport()
        {

            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await reportService.GetProjectReport(GetCurrentUserId());
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

        [HttpGet("GetProjectWiseReport")]
        public async Task<GeneralResponse> GetProjectWiseReport(string projectId)
        {

            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await reportService.GetProjectWiseReport(projectId);
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

        #endregion

        #endregion
    }
}
