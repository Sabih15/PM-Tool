using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Authorization
{
    internal class ActiveUserAuthorizationRequirement : IAuthorizationRequirement
    {
        public bool IsUserActive(string publicId, IRepository<User> userRepository)
        {
            var userId = Guid.Parse(publicId);
            var result = userRepository.GetAll().FirstOrDefault(s => s.UserPublicId == userId);
            return result == null ? false : true;
        }
    }
}
