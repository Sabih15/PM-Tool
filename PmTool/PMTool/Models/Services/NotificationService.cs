using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PMTool.Hubs;
using PMTool.Models.DTOs;
using PMTool.Models.Enums;
using PMTool.Models.Request;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly IHubContext<NotificationHub> notificationHub;
        private readonly IRepository<NotificationType> notificationTypeRepository;
        private readonly IRepository<Notification> notificationRepository;
        private readonly IRepository<NotificationFieldType> notificationFieldTypeRepository;
        private readonly IRepository<NotificationField> notificationFieldRepository;
        private readonly IUserService userService;

        #endregion

        #region Constructors

        public NotificationService(IHubContext<NotificationHub> _notificationHub,
            IRepository<NotificationType> _notificationTypeRepository,
            IRepository<Notification> _notificationRepository,
            IRepository<NotificationFieldType> _notificationFieldTypeRepository,
            IRepository<NotificationField> _notificationFieldRepository,
            IUserService _userService
            )
        {
            notificationHub = _notificationHub;
            notificationTypeRepository = _notificationTypeRepository;
            notificationRepository = _notificationRepository;
            notificationFieldTypeRepository = _notificationFieldTypeRepository;
            notificationFieldRepository = _notificationFieldRepository;
            userService = _userService;
        }

        #endregion

        #region Methods

        public async Task<NotificationDto> GetNotifications(int? currentUserId, int count, int skip)
        {
            try
            {
                var user = userService.GetUserById(currentUserId.Value);
                NotificationDto result = new NotificationDto();
                List<NotificationListDto> notificationDtos = new List<NotificationListDto>();
                var items = notificationRepository.GetAll()
                    .Include(s => s.NotificationType)
                    .Include(s => s.FromUser)
                    .Where(s => s.ToUserId == currentUserId).OrderByDescending(s => s. NotificationId).ToList();
                var total = items.Count();
                var unread = items.Where(s => s.IsRead == false).Count();
                items = items.Skip(skip).Take(count).ToList();
               
                foreach (var item in items)
                {
                    var notificationFields = notificationFieldRepository.GetAll()
                        .Include(s => s.NotificationFieldType)
                        .Include(s => s.Project)
                        .Where(s => s.NotificationId == item.NotificationId).ToList();

                    var totalMins = (DateTime.Now - item.CreatedDate.Value).TotalMinutes;
                    var totalMinsStr = "";
                    if (totalMins <= 5)
                    {
                        totalMinsStr = "Just Now";
                    }
                    else if (totalMins > 5 && totalMins < 60)
                    {
                        totalMinsStr = ((int)totalMins).ToString() + " minutes ago";
                    }
                    else if (totalMins >= 60 && totalMins < 1440) //Less than a  day
                    {
                        int hrs = (int)totalMins / 60;
                        totalMinsStr = hrs.ToString() + " hours ago";
                    }
                    else
                    {
                        int days = (int)totalMins / 1440;
                        totalMinsStr = days.ToString() + " days ago";
                    }

                    var dto = new NotificationListDto()
                    {
                        IsRead = item.IsRead,
                        NotificationId = item.NotificationId,
                        Url = item.NotificationType.Url,
                        TimeAgo = totalMinsStr
                    };
                    dto.Text = item.NotificationType.Template;
                    dto.Text = dto.Text.Replace("{ToUser}", user.FullName);
                    dto.Text = dto.Text.Replace("{FromUser}", item.FromUser.FullName);
                    foreach (var field in notificationFields)
                    {
                        if (field.NotificationFieldTypeId != null)
                            dto.Text = dto.Text.Replace(field.NotificationFieldType.Text, field.Value);
                        if (field.ProjectId != null)
                            dto.Url = dto.Url.Replace("{ProjectId}", field.Project.ProjectPublicId.ToString());
                        if (field.CardId != null)
                            dto.Url = dto.Url.Replace("{CardId}", field.CardId.ToString());

                    }
                    notificationDtos.Add(dto);
                }
                result.UnreadCount = unread;
                result.TotalCount = total;
                result.Notifications = notificationDtos;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> GetNotificationText(int notificationId)
        {
            try
            {
                var notification = notificationRepository.GetAll().Include(s => s.NotificationType).FirstOrDefault(s => s.NotificationId == notificationId);
                if (notification == null)
                    return null;
                var notificationText = notification.NotificationType.Template;
                if (string.IsNullOrEmpty(notificationText))
                    return null;

                if (notificationText.Contains("{FromUser}"))
                {
                    var fromUser = userService.GetUserById(notification.FromUserId.Value);
                    notificationText = notificationText.Replace("{FromUser}", (fromUser != null ? fromUser.FullName : ""));
                }
                if (notificationText.Contains("{ToUser}"))
                {
                    var toUser = userService.GetUserById(notification.ToUserId.Value);
                    notificationText = notificationText.Replace("{ToUser}", (toUser != null ? toUser.FullName : ""));
                }

                var fields = notificationFieldRepository.GetAll().Where(s => s.NotificationId == notificationId).ToList();
                if (fields.Count > 0)
                {
                    var fieldTypes = await notificationFieldTypeRepository.GetAllListAsync();
                    foreach (var field in fields)
                    {
                        if (field.NotificationFieldTypeId == null)
                            continue;
                        var type = fieldTypes.First(s => s.NotificationFieldTypeId == field.NotificationFieldTypeId);
                        notificationText = notificationText.Replace(type.Text, field.Value);
                    }
                }
                return notificationText;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GetNotificationUrl(int notificationId)
        {
            try
            {
                var notification = notificationRepository.GetAll().Include(s => s.NotificationType).FirstOrDefault(s => s.NotificationId == notificationId);
                if (notification == null)
                    return null;
                var notificationUrl = notification.NotificationType.Url;
                if (string.IsNullOrEmpty(notificationUrl))
                    return null;

                if (notificationUrl.Contains("{CardId}"))
                {
                    var notificationField = notificationFieldRepository.GetAll()
                        .Include(s => s.Card)
                        .Include(s => s.Project)
                        .FirstOrDefault(s => s.NotificationId == notificationId && s.CardId != null);
                    notificationUrl = notificationUrl.Replace("{CardId}", (notificationField != null ? notificationField.CardId.ToString(): ""));
                    notificationUrl = notificationUrl.Replace("{ProjectId}", (notificationField != null ? notificationField.Project.ProjectPublicId.ToString(): ""));
                }
                else if (notificationUrl.Contains("{ProjectId}"))
                {
                    var notificationField = notificationFieldRepository.GetAll()
                        .Include(s => s.Project)
                        .FirstOrDefault(s => s.NotificationId == notificationId && s.ProjectId != null);
                    notificationUrl = notificationUrl.Replace("{ProjectId}", (notificationField != null ? notificationField.Project.ProjectPublicId.ToString() : ""));
                }
                return notificationUrl;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendAsync(string method, string userId, object data)
        {
            try
            {
                if (NotificationHub.Connections.ContainsKey(userId))
                {

                var clients = NotificationHub.Connections[userId];
                await notificationHub.Clients.Clients(clients).SendAsync(method, data);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public NotificationType GetNotificationTypeId(string name)
        {
            try
            {
                var item = notificationTypeRepository.GetAll().FirstOrDefault(s => s.Type == name);
                return item;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> GenerateNotification(AddNotificationRequest addNotificationRequest)
        {
            try
            {
                var notification = new Notification
                {
                    CreatedBy = addNotificationRequest.FromUserId,
                    FromUserId = addNotificationRequest.FromUserId,
                    IsRead = false,
                    NotificationTypeId = addNotificationRequest.NotificationTypeId,
                    ToUserId = addNotificationRequest.ToUserId,
                };
                notification.NotificationId = notificationRepository.InsertAndGetId(notification);

                if (addNotificationRequest.Fields.Count > 0)
                {
                    foreach (var item in addNotificationRequest.Fields)
                    {
                        item.CreatedBy = addNotificationRequest.FromUserId;
                        item.NotificationId = notification.NotificationId;
                        notificationFieldRepository.InsertAndGetId(item);
                    }
                }
                return notification.NotificationId;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task MarkAsRead(int notificationId, int? currentUserId)
        {
            try
            {
                var notification = await notificationRepository.Get(notificationId);
                notification.UpdatedBy = currentUserId;
                notification.IsRead = true;
                await notificationRepository.Update(notification);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
    }
}
