using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class FaqCategoryDto
    {
        public string CategoryName { get; set; }
        public List<FaqDto> Faqs { get; set; }
    }
    public class FaqDto
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
