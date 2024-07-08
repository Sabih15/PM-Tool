using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PMTool.Models.General
{
    public class CustomEmail<T>
    {
        public static async Task<bool> SendEmailToMe(string fromEmail, string toEmail, string body, string host, int port)
        {
            int i = 1;
            while (true)
            {
                try
                {
                    var message = new MailMessage();
                    message.From = new MailAddress(fromEmail);
                    message.Body = body;
                    message.IsBodyHtml = false;
                    message.To.Add(toEmail);
                    string Host = host;
                    int Port = port;
                    using (var smtpClient = new SmtpClient(host, port))
                    {
                        await smtpClient.SendMailAsync(message);
                        return true;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public static async Task<bool> SendVerificationEmail(string FullName, string EmailAddress, string VerificationLink, ILogger<T> logger, IConfiguration config, IWebHostEnvironment environment)
        {
            int i = 1;
            while (true)
            {
                try
                {
                    string smsg = Email<T>.GetTemplateString((int)Helper.EmailTemplates.VerifyEmail, environment);
                    smsg = smsg.Replace("{User_Name}", FullName);
                    smsg = smsg.Replace("{Verify_Key}", VerificationLink);
                    await Email<T>.SendMail(EmailAddress, "Account Verification Email", smsg, "", logger, config);
                    return true;
                }

                catch (Exception ex)
                {
                    logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                    i++;
                    if (i > 3)
                        return false;
                }
            }

        }
        public static async Task<bool> SendInviteEmail(string EmailAddress, string InviteForTeam, string InvitationLink, ILogger<T> logger, IConfiguration config, IWebHostEnvironment environment)
        {
            try
            {
                var FullName = EmailAddress.Split('@')[0];
                if (FullName.Contains('.'))
                    FullName = FullName.Replace('.', ' ');
                string smsg = Email<T>.GetTemplateString((int)Helper.EmailTemplates.InviteEmail, environment);
                smsg = smsg.Replace("{User_Name}", FullName);
                smsg = smsg.Replace("{Team_Name}", InviteForTeam);
                smsg = smsg.Replace("{Invitation_Link}", InvitationLink);
                await Email<T>.SendMail(EmailAddress, "Invitation Email", smsg, "", logger, config);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                return false;
            }
        }

        public static async Task<bool> SendResetEmail(string FullName, string EmailAddress, string VerificationKey, ILogger<T> logger, IConfiguration config, IWebHostEnvironment environment)
        {
            int i = 1;
            while (true)
            {
                try
                {
                    string smsg = Email<T>.GetTemplateString((int)Helper.EmailTemplates.ForgotPassword, environment);
                    smsg = smsg.Replace("{User_Name}", FullName);
                    smsg = smsg.Replace("{Verify_Key}", VerificationKey);
                    await Email<T>.SendMail(EmailAddress, "Reset Password Email", smsg, "", logger, config);
                    return true;
                }

                catch (Exception ex)
                {
                    logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                    i++;
                    if (i > 3)
                        return false;
                }
            }

        }

        public static async Task<bool> SendReminder(string FullName, string EmailAddress, int TemplateId, string Day, string CardName, string ProjectName, string DueTime, string RedirectLink, ILogger<T> logger, IConfiguration config, IWebHostEnvironment environment)
        {
            int i = 1;
            while (true)
            {
                try
                {
                    string smsg = Email<T>.GetTemplateString(TemplateId, environment);
                    smsg = smsg.Replace("{User_Name}", FullName);
                    smsg = smsg.Replace("{Day}", Day);
                    smsg = smsg.Replace("{Card_Name}", CardName);
                    smsg = smsg.Replace("{Project_Name}", ProjectName);
                    smsg = smsg.Replace("{Due_Time}", DueTime);
                    smsg = smsg.Replace("{Redirect_Link}", RedirectLink);
                    await Email<T>.SendMail(EmailAddress, "Card Reminder", smsg, "", logger, config);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                    i++;
                    if (i > 3)
                        return false;
                }
            }
        }
    }
}
