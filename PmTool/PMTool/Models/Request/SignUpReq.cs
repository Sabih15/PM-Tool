using AutoMapper;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    [AutoMap(typeof(User))]
    public class SignUpReq
    {
        public bool IsSocialUser { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PictureURL { get; set; }
        public string Gender { get; set; }
    }
}
