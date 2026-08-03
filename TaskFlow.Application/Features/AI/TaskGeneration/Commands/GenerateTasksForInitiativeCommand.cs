using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.TaskGeneration;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Commands
{
    public sealed class GenerateTasksForInitiativeCommand
        : IRequest<GeneratedTasksPreview>
    {
        public GenerateTasksForInitiativeRequest Request { get; }

        public GenerateTasksForInitiativeCommand(
            GenerateTasksForInitiativeRequest request)
        {
            Request = request;
        }
    }
}
