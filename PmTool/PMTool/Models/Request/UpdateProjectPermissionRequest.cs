using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class UpdateProjectPermissionRequest
    {
        public string Email { get; set; }
        public int PermissionId { get; set; }
    }
}
