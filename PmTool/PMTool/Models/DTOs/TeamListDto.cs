using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class TeamListDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public string Description { get; set; }
        public List<object> TotalMembers { get; set; }
        public bool IsOwner { get; set; }
        public bool? ShowMore { get; set; }
        public int ProjectCount { get; set; }
        public List<GeneralDto> Projects { get; set; }
    }
}
