using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ProjectTeam : BaseEntity
    {
        public int ProjectTeamId { get; set; }
        public int? ProjectId { get; set; }
        public Project Project { get; set; }
        public int? TeamId { get; set; }
        public Team Team { get; set; }
    }
}
