using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ActivityLog : BaseEntity
    {
        public int ActivityLogId { get; set; }
        public int? FromListId { get; set; }
        public ChallengeList FromList { get; set; }
        public int? ToListId { get; set; }
        public ChallengeList ToList { get; set; }
        public int? CardId { get; set; }
        public Card Card { get; set; }
        public string ActivityText { get; set; }
        public DateTime ActivityDateTime { get; set; }
    }
}
