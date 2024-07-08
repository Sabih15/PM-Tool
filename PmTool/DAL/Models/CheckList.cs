using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CheckList : BaseEntity
    {
        public int CheckListId { get; set; }
        public string CheckListName { get; set; }
        public bool? IsCompleted { get; set; }
        public int?  CardId { get; set; }
        public Card Card { get; set; }
        public int? ParentCheckListId { get; set; }
        public CheckList ParentCheckList { get; set; }
        public int? GroupId { get; set; }
    }
}
