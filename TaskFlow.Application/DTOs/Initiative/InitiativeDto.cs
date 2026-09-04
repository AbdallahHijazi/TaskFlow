using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.Initiative
{
    public class InitiativeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Progress { get; set; }
        public bool? IsAISuggested { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public Guid? StatusId { get; set; }
        public string? StatusName { get; set; }
        public Guid? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public Guid? ImageId { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? ImageFileName { get; set; }
        public long? ImageSizeInBytes { get; set; }
        public int TaskCount { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public IFormFile? Image { get; set; }

    }
}
