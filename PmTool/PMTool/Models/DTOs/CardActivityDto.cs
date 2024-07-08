using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class CardActivityDto
    {
        public int ActivityId { get; set; }
        public string MemberName { get; set; }
        public string CardName { get; set; }
        public string FromListName { get; set; }
        public string ToListName { get; set; }
        public string ActivityText { get; set; }
        public DateTime? ActivityDateTime { get; set; }
    }
}
