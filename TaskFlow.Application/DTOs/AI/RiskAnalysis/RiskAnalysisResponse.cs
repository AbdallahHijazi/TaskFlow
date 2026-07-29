using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.RiskAnalysis
{
    public sealed class RiskAnalysisResponse
    {
        public Guid InitiativeId { get; set; }

        public string InitiativeName { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<RiskAnalysisItem> Risks { get; set; } = new();
    }
}
