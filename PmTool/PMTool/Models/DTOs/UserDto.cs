using AutoMapper;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    [AutoMap(typeof(User))]
    public class UserDto //fix column names, same as table column names 
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid? PublicId { get; set; }
        public string RoleName { get; set; }
        public string Picture { get; set; }
        public bool IsVerified { get; set; }
    }
}
