using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Notification : BaseEntity
    {
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public int? ToUserId { get; set; }
        public User ToUser { get; set; }
        public int? FromUserId { get; set; }
        public User FromUser { get; set; }
        public int? NotificationTypeId { get; set; }
        public NotificationType NotificationType { get; set; }

    }
}
