using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Initiative;

namespace TaskFlow.Application.Features.Initiatives.Commands
{
    public class UpdateInitiativeCommand : IRequest<InitiativeDto>
    {
        public Guid Id { get; set; }
        public CreateInitiativeDto Dto { get; set; }

        public UpdateInitiativeCommand(Guid id, CreateInitiativeDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
