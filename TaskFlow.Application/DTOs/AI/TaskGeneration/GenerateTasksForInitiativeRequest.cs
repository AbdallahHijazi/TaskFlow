using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.TaskGeneration
{
    public sealed class GenerateTasksForInitiativeRequest
    {
        public Guid InitiativeId { get; set; }

        public string Prompt { get; set; } = string.Empty;

    }
}
