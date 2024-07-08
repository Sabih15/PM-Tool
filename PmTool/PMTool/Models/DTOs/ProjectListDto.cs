using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectListDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TotalChallenges { get; set; }
        public List<object> TotalMembers { get; set; }
        public int TotalTasks { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Progress { get; set; }
        public bool isOwner { get; set; }
    }
}
