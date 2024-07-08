using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class NotificationField : BaseEntity
    {
        public int NotificationFieldId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int? NotificationId { get; set; }
        public Notification Notification { get; set; }
        public int? NotificationFieldTypeId { get; set; }
        public NotificationFieldType NotificationFieldType { get; set; }
        public int? ProjectId { get; set; }
        public Project Project { get; set; }
        public int? CardId { get; set; }
        public Card Card { get; set; }
        public int? ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
    }
}
