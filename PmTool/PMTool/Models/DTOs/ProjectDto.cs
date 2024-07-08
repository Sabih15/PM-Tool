using AutoMapper;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Resources.Models
{
    [AutoMap(typeof(Project))]
    public class ProjectDto
    {
        public string ProjectName { get; set; }
        public Guid ProjectPublicId { get; set; }
        public string Description { get; set; }
        public int? TotalChallenges { get; set; }
    }
}
