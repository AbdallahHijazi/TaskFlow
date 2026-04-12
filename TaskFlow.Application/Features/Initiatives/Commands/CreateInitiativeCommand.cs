using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Initiative;

namespace TaskFlow.Application.Features.Initiatives.Commands
{
    public class CreateInitiativeCommand : IRequest<InitiativeDto>
    {
        public CreateInitiativeDto Dto { get; set; }

        public CreateInitiativeCommand(CreateInitiativeDto dto)
        {
            Dto = dto;
        }
    }
}
