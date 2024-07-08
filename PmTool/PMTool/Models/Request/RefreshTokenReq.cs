using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class RefreshTokenReq
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
