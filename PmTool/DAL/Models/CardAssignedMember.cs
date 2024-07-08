using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CardAssignedMember : BaseEntity
    {
        public int CardAssignedMemberId { get; set; }
        public int? CardId { get; set; }
        public Card Card { get; set; }
        public int? MemberUserId { get; set; }
        public User MemberUser { get; set; }
    }
}
