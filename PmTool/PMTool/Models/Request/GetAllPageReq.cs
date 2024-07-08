using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class GetAllPageReq
    {
        public string Query { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }
}
