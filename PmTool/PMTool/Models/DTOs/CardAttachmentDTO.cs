using System;
using DAL.Models;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    [AutoMap(typeof(CardAttachment))]
    public class CardAttachmentDTO
    {
        public int AttachmentId { get; set; }
        public string Name { get; set; }
        public string FileUrl { get; set; }
        public string FileExtension { get; set; }
        public decimal? SizeInKB { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
