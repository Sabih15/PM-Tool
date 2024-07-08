using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ChallengeList : BaseEntity
    {
        public int ChallengeListId { get; set; }
        [MaxLength(50)]
        public string ChallengeListName { get; set; }
        public int? ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
    }
}
