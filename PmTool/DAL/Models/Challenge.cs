using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Challenge : BaseEntity
    {
        public int ChallengeId { get; set; }
        [MaxLength(30)]
        public string ChallengeName { get; set; }
        [MaxLength(260)]
        public string Description { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsLocked { get; set; }
        public DateTime? CompleteDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public double? TotalWorkingDurationInMinutes { get; set; }
        public int? ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
