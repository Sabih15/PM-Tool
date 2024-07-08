using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ProjectMember : BaseEntity
    {
        public int ProjectMemberId { get; set; }
        public Guid? ProjectMemberPublicId { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int? ProjectMemberUserId { get; set; }
        public User ProjectMemberUser { get; set; }
    }
}
