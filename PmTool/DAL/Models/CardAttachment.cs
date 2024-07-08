using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CardAttachment : BaseEntity
    {
        public int CardAttachmentId { get; set; }
        public int? CardId { get; set; }
        public Card Card { get; set; }
        public string Name { get; set; }
        public string FileUrl { get; set; }
        public string FileShareableUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string FileExtension { get; set; }
        public decimal? SizeInKB { get; set; }

    }
}
