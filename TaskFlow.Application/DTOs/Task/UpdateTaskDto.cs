using Microsoft.AspNetCore.Http;

namespace TaskFlow.Application.DTOs.Task
{
    public class UpdateTaskDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Progress { get; set; } = 0;
        public Guid StatusId { get; set; }
        public Guid? InitiativeId { get; set; }
        public Guid AssignedToId { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        public string Icon { get; set; } = "";
        public IFormFile? Image { get; set; }
    }
}
