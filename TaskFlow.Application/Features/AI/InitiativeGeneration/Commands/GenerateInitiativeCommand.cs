using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Commands
{
    public sealed class GenerateInitiativeCommand
                                                    : IRequest<GeneratedInitiativePreview>
    {
        public GenerateInitiativeRequest Request { get; }

        public GenerateInitiativeCommand(
            GenerateInitiativeRequest request)
        {
            Request = request;
        }
    }
}
