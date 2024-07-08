using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class TeamMember : BaseEntity
    {
        public int TeamMemberId { get; set; }
        public Guid? TeamMemberPublicId { get; set; }
        public int? TeamId { get; set; }
        public Team Team { get; set; }
        public int? MemberUserId { get; set; }
        public User MemberUser { get; set; }
    }
}
