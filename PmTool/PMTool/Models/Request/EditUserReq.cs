using AutoMapper;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    [AutoMap(typeof(User))]
    public class EditUserReq
    {
        public Guid? UserPublicId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string SocialUser { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string Picture { get; set; }
        public int? Roleid { get; set; }
    }
}
