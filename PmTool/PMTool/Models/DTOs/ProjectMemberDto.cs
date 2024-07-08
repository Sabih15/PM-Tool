using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectMemberDto
    {
        public string TeamName { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public string Picture { get; set; }
        public bool isIndividualMember { get; set; }
    }
}
