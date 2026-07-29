using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.DTOs.AI.RiskAnalysis
{
    public class RiskDto
    {
        public string RiskTitle { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string Impact { get; set; } = string.Empty;

        public string Recommendation { get; set; } = string.Empty;
    }
}
