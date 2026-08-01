using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration
{
    public sealed class GenerateInitiativeRequest
    {
        public string Prompt { get; set; } = string.Empty;

        public Guid StatusId { get; set; }

        public Guid AssignedToId { get; set; }
    }
}
