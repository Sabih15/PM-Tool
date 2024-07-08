using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectPermissionAllowedDto
    {
        public string PermissionCode { get; set; }
        public bool IsAllowed { get; set; }
    }
}
