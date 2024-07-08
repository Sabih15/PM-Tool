using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class AddNotificationRequest
    {
        public int? ToUserId { get; set; }
        public int? FromUserId { get; set; }
        public int? NotificationTypeId { get; set; }
        public List<NotificationField> Fields { get; set; }
    }
}
