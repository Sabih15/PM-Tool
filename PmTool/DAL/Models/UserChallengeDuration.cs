using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class UserChallengeDuration : BaseEntity
    {
        public int UserChallengeDurationId { get; set; }
        public int? ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public float? Duration { get; set; }
    }
}
