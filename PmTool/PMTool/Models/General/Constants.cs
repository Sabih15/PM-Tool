using PMTool.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.General
{
    public static class Constants
    {
        public static string AUTH_FORGOT_SUCCESS = "Your account has been successfully reset";
        public static string AUTH_FORGOT_INVALIDCODE = "Reset code is incorrect";
        public static string AUTH_FORGOT_RESENDCODE_SUCCESS = "Reset code has been resent to your email";
        public static string AUTH_FORGOT_RESENDCODE_ERROR = "Reset code you entered is incorrect";
        public static string AUTH_VERIFICATION_RESEND_SUCCESS = "Verification code has been resent to your email";
        public static string AUTH_VERIFICATION_RESEND_ERROR = "There was an error sending you the email";
        public static string AUTH_VERIFICATION_SUCCESS = "Your email has been successfully verified";
        public static string AUTH_VERIFICATION_INVALID = "Verification code you entered is incorrect";
        public static string PROJECT_CREATE_SUCCESS = "Project created successfully";
        public static string PROJECT_CREATE_ERROR = "There was an error creating your project";
        public static string PROJECT_UPDATE_SUCCESS = "Project updated successfully";
        public static string PROJECT_UPDATE_ERROR = "There was an error updating your project";
        public static string USER_UPDATE_SUCCESS = "Account updated successfully";
        public static string CARD_MEMBER_ASSIGN = "A member added to the card";
        public static string CARD_MEMBER_UNASSIGN = "A member removed from the card";
        public static string CARD_STATUS_CHANGE = "Card status has been updated";
        public static string CARD_DUE_DATE_CHANGE = "Due-date of the card has been updated";
        public static string CARD_ATTACHMENT_ADD= "Attachment added successfully";
        public static string FEEDBACK_ADD= "Thanks you for your feedback. Out expert will get back to you soon.";
        public static string FEEDBACK_SUBJECT= "Thanks you for your feedback. Out expert will get back to you soon.";
        public static string FEEDBACK_ADD_FAIL= "The email could not be sent. Try using another email or try again later";
        public static string PROJECT_PERMISSIONS_UPDATE= "Project permissions successfully updated.";

        public static string ContentRootPath = "";

        private static Dictionary<string, List<string>> fileTypes = new Dictionary<string, List<string>>()
        {
            {FileTypes.Image.ToString(), new List<string>() {"ico", "jpg", "jpeg", "png", "svg" } },
            {FileTypes.Pdf.ToString(), new List<string>() { "pdf" } },
            {FileTypes.Word.ToString(), new List<string>() { "doc", "docx" } },
            {FileTypes.Excel.ToString(), new List<string>() { "xls", "xlsx" } }
        };

        public static string GetFileTypeByExtension(string extension)
        {
            string result = null;
            foreach (var entry in fileTypes)
            {
                if (entry.Value.Contains(extension.ToLower()))
                {
                    result = entry.Key;
                    break;
                }
            }
            return result;
        }
    }
}
