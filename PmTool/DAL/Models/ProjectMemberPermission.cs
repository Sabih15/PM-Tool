using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ProjectMemberPermission : BaseEntity
    {
        public int ProjectMemberPermissionId { get; set; }
        public int MemberUserId { get; set; }
        public User MemberUser { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int ProjectPermissionId { get; set; }
        public ProjectPermission ProjectPermission { get; set; }
    }
}
