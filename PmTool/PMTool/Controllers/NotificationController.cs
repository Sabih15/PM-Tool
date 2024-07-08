using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.Models.Enums;
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
    public class NotificationController : BaseController
    {
        #region Fields

        private readonly INotificationService notificationService;
        private readonly ILogger<NotificationController> logger;
        private readonly IUserService userService;

        #endregion

        #region Constructors

        public NotificationController(INotificationService _notificationService, ILogger<NotificationController> _logger, IUserService _userService)
        {
            notificationService = _notificationService;
            logger = _logger;
            userService = _userService;
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

        [HttpGet("GetNotifications")]
        public async Task<GeneralResponse> GetNotifications(int count, int skip)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await notificationService.GetNotifications(GetCurrentUserId(), count, skip);
                if (result != null)
                {
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                    response.Data = result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("MarkAsRead")]
        public async Task<GeneralResponse> MarkAsRead(int notificationId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await notificationService.MarkAsRead(notificationId, GetCurrentUserId());
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
