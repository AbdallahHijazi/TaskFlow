using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.CriticalTaskAnalysis;

namespace TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Commands
{
    public sealed class AnalyzeCriticalTasksCommand
        : IRequest<CriticalTasksAnalysisResponse>
    {
        public AnalyzeCriticalTasksRequest Request { get; }

        public AnalyzeCriticalTasksCommand(
            AnalyzeCriticalTasksRequest request)
        {
            Request = request;
        }
    }
}
