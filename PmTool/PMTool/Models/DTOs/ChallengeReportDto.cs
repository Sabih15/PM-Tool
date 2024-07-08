using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ChallengeReportDto
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Duration { get; set; }
        public List<MembersReportDto> Members { get; set; }
    }
}
