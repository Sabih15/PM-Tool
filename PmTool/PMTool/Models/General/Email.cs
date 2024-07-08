using System.Net.Mail;
using System.IO;
using System.Collections.Generic;
using System;
using System.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.Controllers;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace PMTool.General
{
    public class Email<T>
    {
        public static string baseUrl = ConfigurationManager.AppSettings["EmailBaseURL"];
        public static async Task SendMail(string to, string subject, string msg, string cc, ILogger<T> logger, IConfiguration config)
        {
            try
            {
                logger.LogInformation("sending email");
                baseUrl = config["EmailConfiguration:EmailBaseURL"];
                msg = msg.Replace("{BaseUrl}", baseUrl);

                string SenderEmailAddress = config["EmailConfiguration:SenderEmailAddress"];
                string SenderEmailPassword = config["EmailConfiguration:SenderEmailPassword"];
                string SenderSMTPServer = config["EmailConfiguration:SenderSMTPServer"];
                int Port = Convert.ToInt32(config["EmailConfiguration:Port"]);
                bool IsSsl = Convert.ToBoolean(config["EmailConfiguration:IsSsl"]);
                string DisplayName = config["EmailConfiguration:DisplayName"];
                bool IsLive = Convert.ToBoolean(config["EmailConfiguration:IsLive"]);
                MailMessage message = new MailMessage();
                string[] addresses = to.Split(';');
                foreach (string address in addresses)
                {
                    message.To.Add(new MailAddress(address));
                }

                if (string.IsNullOrEmpty(cc) == false)
                    message.CC.Add(new MailAddress(cc));


                if (IsLive == false)
                {

                }
                //message.Bcc.Add("saba.aimviz@gmail.com");

                message.From = new MailAddress(SenderEmailAddress, DisplayName);
                message.Subject = subject;
                message.Body = msg;
                message.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = SenderSMTPServer;
                if (Port > 0)
                    client.Port = Port;
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(SenderEmailAddress, SenderEmailPassword);
                client.EnableSsl = IsSsl;
                client.Credentials = nc;
                await client.SendMailAsync(message);
                logger.LogInformation("Email Sent");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                //Logging.WriteLog(Logging.LogType.Error, ex.TargetSite.Name + " - " + ex.Message);
            }
        }

        public static void SendBulkMail(List<string> toBCC, string subject, string msg, string cc)
        {
            try
            {
                string SenderEmailAddress = System.Configuration.ConfigurationManager.AppSettings["SenderEmailAddress"];
                string SenderEmailPassword = System.Configuration.ConfigurationManager.AppSettings["SenderEmailPassword"];
                string SenderSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SenderSMTPServer"];
                int Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Port"]);
                bool IsSsl = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsSsl"]);
                string DisplayName = System.Configuration.ConfigurationManager.AppSettings["DisplayName"];

                MailMessage message = new MailMessage();

                foreach (string address in toBCC)
                {
                    message.Bcc.Add(new MailAddress(address));
                }

                //message.Bcc.Add("saba.aimviz@gmail.com");
                if (string.IsNullOrEmpty(cc) == false)
                    message.CC.Add(new MailAddress(cc));

                message.From = new MailAddress(SenderEmailAddress, DisplayName);
                message.Subject = subject;
                message.Body = msg;
                message.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = SenderSMTPServer;
                if (Port > 0)
                    client.Port = Port;
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(SenderEmailAddress, SenderEmailPassword);
                client.EnableSsl = IsSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = nc;
                client.Send(message);
            }
            #pragma warning disable 0168
            catch (Exception ex)
            #pragma warning restore 0168
            {

                return;
            }
        }

        public static void SendMailWithAttachment(string to, string subject, string msg, string cc, string filepath, string FileName)
        {
            try
            {

                string SenderEmailAddress = System.Configuration.ConfigurationManager.AppSettings["SenderEmailAddress"];
                string SenderEmailPassword = System.Configuration.ConfigurationManager.AppSettings["SenderEmailPassword"];
                string SenderSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SenderSMTPServer"];
                int Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Port"]);
                bool IsSsl = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsSsl"]);
                string DisplayName = System.Configuration.ConfigurationManager.AppSettings["DisplayName"];
                bool IsLive = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsLive"]);

                MailMessage message = new MailMessage();
                string[] addresses = to.Split(';');
                foreach (string address in addresses)
                {
                    message.To.Add(new MailAddress(address));
                }

                if (string.IsNullOrEmpty(cc) == false)
                    message.CC.Add(new MailAddress(cc));

                //message.Bcc.Add("saba.aimviz@gmail.com");
                if (IsLive == false)
                {
                }

                message.From = new MailAddress(SenderEmailAddress, DisplayName);
                message.Subject = subject;
                message.Body = msg;
                message.IsBodyHtml = true;

                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(filepath);
                attachment.Name = FileName;  // set name here
                message.Attachments.Add(attachment);

                SmtpClient client = new SmtpClient();
                client.Host = SenderSMTPServer;
                if (Port > 0)
                    client.Port = Port;
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(SenderEmailAddress, SenderEmailPassword);
                client.EnableSsl = IsSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = nc;
                client.Send(message);
            }
            catch (Exception ex)
            {
                Logging.WriteLog(Logging.LogType.Error, ex.TargetSite.Name + " - " + ex.Message);
            }
        }

        public static string GetTemplateString(int templateCode, IWebHostEnvironment environment)
        {
            StreamReader objStreamReader;
            string path = "";
            if (templateCode == (int)Helper.EmailTemplates.ForgotPassword)
            {
                path = environment.ContentRootPath + @"\Templates\ForgotPassword.html";
            }
            else if (templateCode == (int)Helper.EmailTemplates.VerifyEmail)
            {
                path = environment.ContentRootPath + @"\Templates\Verification.html";
            }
            else if (templateCode == (int)Helper.EmailTemplates.InviteEmail)
            {
                path = environment.ContentRootPath + @"\Templates\Invitation.html";
            }
            else if (templateCode == (int)Helper.EmailTemplates.CardDueToday)
            {
                path = environment.ContentRootPath + @"\Templates\CardReminder.html";
            }
            else if (templateCode == (int)Helper.EmailTemplates.CardDueTomorrow)
            {
                path = environment.ContentRootPath + @"\Templates\CardReminder.html";
            }
            else if (templateCode == (int)Helper.EmailTemplates.ProjectDueToday)
            {
                path = environment.ContentRootPath + @"\Templates\ProjectReminder.html";
            }
            else if (templateCode == (int)Helper.EmailTemplates.ProjectDueTomorow)
            {
                path = environment.ContentRootPath + @"\Templates\ProjectReminder.html";
            }
            if (!string.IsNullOrEmpty(path))
            {
                objStreamReader = File.OpenText(path);
                string emailText = objStreamReader.ReadToEnd();
                objStreamReader.Close();
                objStreamReader = null;
                objStreamReader = null;
                return emailText;

            }
            else
            {
                objStreamReader = null;
                return string.Empty;
            }


        }
    }
}
