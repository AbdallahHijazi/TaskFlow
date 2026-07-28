using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.TaskGeneration
{
    public sealed class SaveGeneratedTasksResponse
    {
        public Guid InitiativeId { get; set; }

        public int CreatedTasksCount { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
