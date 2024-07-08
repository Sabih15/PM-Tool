using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class NotificationFieldType : BaseEntity
    {
        public int NotificationFieldTypeId { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
    }
}
