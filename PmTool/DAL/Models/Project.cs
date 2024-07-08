using System;
using System.ComponentModel.DataAnnotations;


namespace DAL.Models
{
    public class Project : BaseEntity
    {
        public int ProjectId { get; set; }
        [MaxLength(30)]
        public string ProjectName { get; set; }
        public Guid? ProjectPublicId { get; set; }
        public string Description { get; set; }
        public int? TotalChallenges { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
