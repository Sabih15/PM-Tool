using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectListDtoPage
    {
        public List<ProjectListDto> ProjectList { get; set; }
        public int Count { get; set; }
    }
}
