using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class NotificationType : BaseEntity
    {
        public int NotificationTypeId { get; set; }
        public string Type { get; set; }
        public string Template { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
    }
}
