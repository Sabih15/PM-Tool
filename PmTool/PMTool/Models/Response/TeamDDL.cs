using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Response
{
    public class TeamDDL
    {
        public string TeamName { get; set; }
        public List<string> MemberEmails { get; set; }
    }
}
