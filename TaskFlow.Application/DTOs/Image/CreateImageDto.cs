using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Image
{
    public class CreateImageDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; } 
        public string? ThumbnailPath { get; set; }
    }
}
