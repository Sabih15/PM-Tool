using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Authorization
{
    public static class JwtClaimAccessor
    {
        public static string GetClaimByType(string claimType, HttpContext httpContext)
        {
            var claim = httpContext.User.Claims.FirstOrDefault(s => s.Type == claimType);
            return claim == null ? null : claim.Value;
        }
        
    }
}
