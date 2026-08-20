using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities
{
    public class Image:BaseEntity, ITenantEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? MediaType { get; set; }
        public long? SizeInBytes { get; set; }
        public DateTime? UploadedAt { get; set; }
        public Guid? UploadedById { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? Thumbnail { get; set; }

        public User? UploadedBy { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
