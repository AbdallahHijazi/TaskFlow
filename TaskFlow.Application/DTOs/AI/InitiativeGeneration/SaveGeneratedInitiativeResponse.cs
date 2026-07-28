using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration
{
    public sealed class SaveGeneratedInitiativeResponse
    {
        public Guid InitiativeId { get; set; }

        public int CreatedTasksCount { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
