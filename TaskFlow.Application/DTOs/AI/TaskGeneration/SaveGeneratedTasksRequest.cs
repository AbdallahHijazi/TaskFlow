using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.DTOs.AI.TaskGeneration
{
    public sealed class SaveGeneratedTasksRequest
    {
        public Guid InitiativeId { get; set; }

        public Guid StatusId { get; set; }

        public Guid AssignedToId { get; set; }

        public List<GeneratedTaskPreview> Tasks { get; set; } = new();
    }
}
