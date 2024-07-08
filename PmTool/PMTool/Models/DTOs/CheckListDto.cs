using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class CheckListDto
    {
        public int GroupId { get; set; }
        public CheckListGroupDto checkListGroup { get; set; }
    }

    public class CheckListGroupDto
    {
        public int Progress { get; set; }
        public List<CheckListItemDto> CheckLists { get; set; }
    }

    public class CheckListItemDto
    {
        public int CheckListId { get; set; }
        public bool Status { get; set; }
        public string CheckListText { get; set; }
        public List<CheckListItemDto> SubList { get; set; }
    }
}
