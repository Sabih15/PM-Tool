using System;
using DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class CardCommentsDTO
    {
        public string CommentsText { get; set; }
        public int CommentId{ get; set; }
        public DateTime CommentsDateTime { get; set; }
        public int? CommentBy { get; set; }
        public String CommentByFullName { get; set; }
        public bool IsOwner { get; set; }
    }
}
