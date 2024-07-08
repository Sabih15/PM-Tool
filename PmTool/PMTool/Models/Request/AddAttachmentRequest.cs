using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Request
{
    public class AddAttachmentRequest
    {
        public int CardId { get; set; }
        public long SizeInByte { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileAsBase64 { get; set; }
    }
}
