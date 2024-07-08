using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class ProjectDetailDto
    {
        public int ChallengeId { get; set; }
        public string ChallengeName { get; set; }

        public bool IsLocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOwner { get; set; }
        public string TimeTracked { get; set; }

        public List<ChallengeListDto> ChallengeList { get; set; }

    }

    public class ChallengeListDto
    {
        public int ListId { get; set; }
        public string ListName { get; set; }
        public List<CardListDto> CardList { get; set; }
    }

    public class CardListDto
    {
        public int CardId { get; set; }
        public string CardName { get; set; }
        public DateTime? DueDate { get; set; }
        public int TotalAttachments { get; set; }
        public int CheckListItems { get; set; }
        public int CheckListItemsCompleted { get; set; }
        public string Status { get; set; }
        public string Members { get; set; }
    }
}
