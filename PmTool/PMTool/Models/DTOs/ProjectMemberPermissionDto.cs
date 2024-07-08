using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectMemberPermissionDto
    {
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public int PermissionId { get; set; }
        public string PermissionDisplayName { get; set; }
    }
}
