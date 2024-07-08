using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class AddActivityLogRequest
    {
        public string ActivityDateTime { get; set; }
        public string ActivityText { get; set; }
        public int? FromListId { get; set; }
        public int? ToListId { get; set; }
        public int? CardId { get; set; }
    }
}
