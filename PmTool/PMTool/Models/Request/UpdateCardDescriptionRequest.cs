using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class UpdateCardDescriptionRequest
    {
        public int CardId { get; set; }
        public string Description { get; set; }
    }
}
