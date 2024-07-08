using AutoMapper;
using DAL.Models;
using System;

namespace PMTool.Models.DTOs
{
    [AutoMap(typeof(User))]
    public class UsersDto
    {
        public Guid UserPublicId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string SocialUser { get; set; }
    }
}
