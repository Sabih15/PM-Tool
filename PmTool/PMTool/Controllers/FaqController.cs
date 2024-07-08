using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.Models.General;
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
    public class FaqController : BaseController
    {
        #region Fields

        private readonly IUserService userService;
        private readonly IFaqService faqService;
        private readonly ILogger<FaqController> logger;

        #endregion

        #region Constructors

        public FaqController(IUserService _userService, IFaqService _faqService, ILogger<FaqController> _logger)
        {
            userService = _userService;
            faqService = _faqService;
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

        [HttpGet("GetFaq")]
        public GeneralResponse GetFaqs()
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = faqService.GetFaq();
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

        [HttpPost("AddFeedback")]
        public async Task<GeneralResponse> AddFeedback(AddFeedbackRequest addFeedbackRequest)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await faqService.AddFeedback(addFeedbackRequest, GetCurrentUserId());
                if (result)
                {
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
                    response.Message = Constants.FEEDBACK_ADD;
                }
                else
                {
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
                    response.Message = Constants.FEEDBACK_ADD_FAIL;
                }
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
