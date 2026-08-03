using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.DTOs.AI.TaskGeneration
{
    public sealed class GeneratedTasksPreview
    {
        public Guid InitiativeId { get; set; }

        public string InitiativeName { get; set; } = string.Empty;

        public List<GeneratedTaskPreview> Tasks { get; set; } = new();
    }
}
