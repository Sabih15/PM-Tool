using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class AddCommentRequest
    {
        public string CommentDateTime { get; set; }
        public string CommentText { get; set; }
        public int CardId { get; set; }
        public List<string> ToUsers { get; set; }
    }
}
