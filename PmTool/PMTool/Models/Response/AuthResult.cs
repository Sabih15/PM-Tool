using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Response
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
