using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.CriticalTaskAnalysis
{
    public sealed class CriticalTaskAnalysisItem
    {
        public Guid TaskId { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public int CriticalityScore { get; set; }

        public string CriticalityLevel { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string Recommendation { get; set; } = string.Empty;
    }
}
