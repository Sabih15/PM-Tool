using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class EditProjectDto
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? TotalChallenges { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string> Teams { get; set; }
        public List<string> MemberEmails { get; set; }
        public int? UnlockedChallenges { get; set; }
    }
}
