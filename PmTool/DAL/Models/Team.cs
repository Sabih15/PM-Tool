using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Team : BaseEntity
    {
        public int TeamId { get; set; }
        [MaxLength(30)]
        public string TeamName { get; set; }
        public string Description { get; set; }
        public Guid? TeamPublicId { get; set; }
        public int? TotalMembers { get; set; }
    }
}
