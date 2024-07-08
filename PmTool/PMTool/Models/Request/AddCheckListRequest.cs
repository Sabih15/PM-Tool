using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class AddCheckListRequest
    {
        public string Name { get; set; }
        public int CardId { get; set; }
        public int? ParentCheckListId { get; set; }
        public int GroupId { get; set; }
    }
}
