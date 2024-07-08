using AutoMapper;
using DAL.Models;
using PMTool.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    [AutoMap(typeof(Team))]
    public class UpsertTeamReq
    {
        public Guid? TeamId { get; set; }
        public string TeamName { get; set; }
        public string Description { get; set; }
        public List<string> MemberEmails { get; set; }
        public List<object> Members { get; set; }
    }
}
