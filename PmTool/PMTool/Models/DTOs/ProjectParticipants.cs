using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectParticipants
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string TeamName { get; set; }
        public bool IsTeamMember { get; set; }
    }
}
