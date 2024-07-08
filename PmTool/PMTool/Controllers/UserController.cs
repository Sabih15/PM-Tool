using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PMTool.Authorization;
using PMTool.Models.DTOs;
using PMTool.Models.General;
using PMTool.Models.Request;
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
    public class UserController : BaseController
    {
        #region Field(s)

        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger<UserController> logger;

        #endregion

        #region Contructor(s)

        public UserController(
            IUserService _userService,
            IMapper _mapper,
            ILogger<UserController> _logger
            )
        {
            userService = _userService;
            mapper = _mapper;
            logger = _logger;
        }

        #endregion

        #region Method(s)

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

        [HttpPost("FilteredList")]
        public GeneralResponse FilteredList(string query, int pageSize, int pageIndex)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = userService.GetFilteredList(query, pageSize, pageIndex);
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

        [HttpGet("GetUserById")]
        public GeneralResponse GetUserByPublicId(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var user = userService.GetEditUserById(id);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }

            return response;
        }

        [HttpGet("GetRoles")]
        public async Task<GeneralResponse> GetRoles()
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var roles = await userService.GetRoles();
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = roles;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }

            return response;
        }

        [HttpPut("UpdateUser")]
        public async Task<GeneralResponse> EditUser([FromForm] IFormFile file, [FromForm] string user /*EditUserReq editUserReq, IFormFile file*/)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var editUserReq = JsonConvert.DeserializeObject<EditUserReq>(user);
                var result = await userService.EditUser(editUserReq, GetCurrentUserId(), file);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
                response.Message = Constants.USER_UPDATE_SUCCESS;
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteUser")]
        public async Task<GeneralResponse> DeleteUser(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await userService.DeleteUser(id, GetCurrentUserId());
                if (result != null && result != false)
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);
                else if (result == false)
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.NotFound);
                else
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("GetUpdatedUser")]
        public GeneralResponse GetUpdatedUser(string Id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var item = userService.GetUserByPublicId(Id);
                var result = mapper.Map<UserDto>(item);
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

        #endregion

        #endregion
    }
}
