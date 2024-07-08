using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PMTool.General.Helper;

namespace PMTool.Resources.Response
{
    public class GeneralResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }

        public GeneralResponse()
        {
            Success = true;
            Message = "Successful";
        }

        public static void SetResponse(GeneralResponse response, ResponseEnum type)
        {
            if (type == ResponseEnum.UnAuthorized)
            {
                response.Success = false;
                response.Message = "Unauthorized user.";
            }
            else if (type == ResponseEnum.InvalidInformation)
            {
                response.Success = false;
                response.Message = "Information you provided is invalid.";
            }
            else if (type == ResponseEnum.InvalidLogin)
            {
                response.Success = false;
                response.Message = "Email or password is incorrect";
            }
            else if (type == ResponseEnum.DefaultErrorMsg)
            {
                response.Success = false;
                response.Message = "There was a problem in processing your request, please try again.";
            }
            else if (type == ResponseEnum.GetSuccess)
            {
                response.Success = true;
                response.Message = "Record retrieved successfully";
            }
            else if (type == ResponseEnum.CreateSuccess)
            {
                response.Success = true;
                response.Message = "Record created successfully";
            }
            else if (type == ResponseEnum.UpdateSuccess)
            {
                response.Success = true;
                response.Message = "Record updated successfully";
            }
            else if (type == ResponseEnum.DeleteSuccess)
            {
                response.Success = true;
                response.Message = "Record deleted successfully";
            }
            else if (type == ResponseEnum.SignUpSuccess)
            {
                response.Success = true;
                response.Message = "Your account has been successfully registered";
            }
            else if (type == ResponseEnum.TokenExpired)
            {
                response.Success = false;
                response.Message = "Token expired/revoked, you must login again";
            }
            else if (type == ResponseEnum.UserExist)
            {
                response.Success = false;
                response.Message = "This email is already taken.";
            }
            else if (type == ResponseEnum.NotASocialUser)
            {
                response.Success = false;
                response.Message = "This email exist but not registered with a social provider";
            }
            else if (type == ResponseEnum.IsASocialUser)
            {
                response.Success = false;
                response.Message = "You cannot reset this email's password because it is registered with a social provider";
            }
            else if (type == ResponseEnum.NotFound)
            {
                response.Success = false;
                response.Message = "Could not find the record";
            }
            else if (type == ResponseEnum.EmailNotSent)
            {
                response.Success = false;
                response.Message = "Email could not be sent, please try again in a few moments";
            }
            else if (type == ResponseEnum.ProjectExist)
            {
                response.Success = false;
                response.Message = "You already have a project with this name, please try a different name";
            }
            else if (type == ResponseEnum.TeamExist)
            {
                response.Success = false;
                response.Message = "You already have a team with this name, please try a different name";
            }
            else if (type == ResponseEnum.TeamInvitationExpired)
            {
                response.Success = false;
                response.Message = "Your invitation for this team has been expired";
            }
            else if (type == ResponseEnum.ProjectInvitationExpired)
            {
                response.Success = false;
                response.Message = "Your invitation for this project has been expired";
            }
        }
    }
}
