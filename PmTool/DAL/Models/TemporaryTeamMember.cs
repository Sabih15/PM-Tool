using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class TemporaryTeamMember : BaseEntity
    {
        public int TemporaryTeamMemberId { get; set; }
        public Guid? TemporaryTeamMemberPublicId { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public string Email { get; set; }
        public string InviteLink { get; set; }
    }
}
