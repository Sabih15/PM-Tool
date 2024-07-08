using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectDetailHeaderDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string TimeTracked { get; set; }
        public List<ProjectMemberDto> Members { get; set; }
        public bool IsOwner { get; set; }
    }
}
