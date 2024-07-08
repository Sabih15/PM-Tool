using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Role : BaseEntity
    {
        public int RoleId { get; set; }
        [MaxLength(100)]
        public string RoleName { get; set; }
    }
}
