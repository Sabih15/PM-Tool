using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Comments : BaseEntity
    {
        public int CommentsId { get; set; }
        public int? CardId { get; set; }
        public Card Card { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDateTime { get; set; }
    }
}
