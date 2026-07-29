using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.RiskAnalysis
{
    public sealed class RiskAnalysisItem
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Probability { get; set; } = string.Empty;

        public string Impact { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public List<Guid> AffectedTasks { get; set; } = new();

        public string Recommendation { get; set; } = string.Empty;
    }
}
