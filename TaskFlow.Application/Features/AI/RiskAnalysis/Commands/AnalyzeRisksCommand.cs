using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.RiskAnalysis;

namespace TaskFlow.Application.Features.AI.RiskAnalysis.Commands
{
    public class AnalyzeRisksCommand : IRequest<RiskAnalysisResponse>
    {
        public AnalyzeRisksRequest Request { get; }

        public AnalyzeRisksCommand(AnalyzeRisksRequest request)
        {
            Request = request;
        }
    }
}
