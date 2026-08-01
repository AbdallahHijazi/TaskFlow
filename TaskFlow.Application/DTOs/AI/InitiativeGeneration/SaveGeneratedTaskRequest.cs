using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration
{
    public sealed class SaveGeneratedTaskRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Color { get; set; } = "#4F46E5";

        public string Icon { get; set; } = "task";
    }
}
