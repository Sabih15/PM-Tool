using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectReportDto
    {
        public string Name { get; set; }
        public int ChallengeCount { get; set; }
        public List<ProjectMemberDto> Members { get; set; }
        public int CompletedChallengeCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public Double Progress { get; set; }
        public string Duration { get; set; }
    }
}
