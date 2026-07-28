using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Task
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Progress { get; set; }
        //public int? Priority { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public Guid? StatusId { get; set; }
        public string? StatusName { get; set; }
        public Guid InitiativeId { get; set; }
        public string? InitiativeName { get; set; }
        public Guid? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ImageId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? FilePath { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageContentType { get; set; }
        public long? ImageSizeInBytes { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
        public bool? IsAISuggested { get; set; }
    }
}
