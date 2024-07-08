using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Authorization
{
    internal class ActiveUserAuthorizationHandler : AuthorizationHandler<ActiveUserAuthorizationRequirement>
    {
        private readonly IRepository<User> userRepository;

        public ActiveUserAuthorizationHandler(IRepository<User> _userRepository)
        {
            userRepository = _userRepository;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ActiveUserAuthorizationRequirement requirement)
        {
            // Bail out if the Sub claim isn't present
            var claim = context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier);
            if (claim == null)
                return Task.CompletedTask;

            if (requirement.IsUserActive(claim.Value, userRepository))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
