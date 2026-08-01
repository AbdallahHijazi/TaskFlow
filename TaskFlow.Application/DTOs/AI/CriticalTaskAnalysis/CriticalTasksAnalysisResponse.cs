using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.CriticalTaskAnalysis
{
    public sealed class CriticalTasksAnalysisResponse
    {
        public Guid InitiativeId { get; set; }

        public string InitiativeName { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<CriticalTaskAnalysisItem> CriticalTasks { get; set; } = new();
    }
}
