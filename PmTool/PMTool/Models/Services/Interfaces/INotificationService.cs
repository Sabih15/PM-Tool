using DAL.Models;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public interface INotificationService
    {
        Task<int> GenerateNotification(AddNotificationRequest addNotificationRequest);
        NotificationType GetNotificationTypeId(string name);
        Task SendAsync(string method, string userId, object data);
        Task<NotificationDto> GetNotifications(int? currentUserId, int count, int skip);
        Task MarkAsRead(int notificationId, int? currentUserId);
        Task<string> GetNotificationText(int notificationId);
        string GetNotificationUrl(int notificationId);
    }
}