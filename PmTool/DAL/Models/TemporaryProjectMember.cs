using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class TemporaryProjectMember : BaseEntity
    {
        public int TemporaryProjectMemberId { get; set; }
        public Guid? TemporaryProjectMemberPublicId { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string Email { get; set; }
        public string InviteLink { get; set; }
    }
}
