using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ProjectPermission : BaseEntity
    {
        public int ProjectPermissionId { get; set; }
        public string ProjectPermissionName { get; set; }
        public string ProjectPermissionDescription { get; set; }
        public string ProjectPermissionDisplayName { get; set; }
        public string ProjectPermissionCode { get; set; }
    }
}
