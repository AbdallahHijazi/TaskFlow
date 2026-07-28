using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration
{
    public sealed class GeneratedInitiativePreview
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Color { get; set; } = "#FFFFFF";

        public string Icon { get; set; } = string.Empty;

        public List<GeneratedTaskPreview> Tasks { get; set; } = new();
    }
}
