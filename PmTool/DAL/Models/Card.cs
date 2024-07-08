using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Card : BaseEntity
    {
        public int CardId { get; set; }
        [MaxLength(30)]
        public string CardName { get; set; }
        public string Description { get; set; }
        public int? ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
        public int? ChallengeListId { get; set; }
        public ChallengeList ChallengeList { get; set; }
        public int? CardStatus { get; set; }
        public DateTime? DueDate { get; set; }
    }

    
}
