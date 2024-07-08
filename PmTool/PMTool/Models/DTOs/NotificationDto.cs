using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class NotificationDto
    {
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }

        public List<NotificationListDto> Notifications { get; set; }
    }

    public class NotificationListDto
    {
        public int NotificationId { get; set; }
        public string Text { get; set; }
        public bool IsRead { get; set; }
        public string Url { get; set; }
        public string TimeAgo { get; set; }
    }
}
