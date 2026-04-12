using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Status;

namespace TaskFlow.Application.Features.Statuses.Commands
{
    public class UpdateStatusCommand : IRequest<StatusDto>
    {
        public Guid Id { get; set; }
        public UpdateStatusDto Dto { get; set; }

        public UpdateStatusCommand(Guid id, UpdateStatusDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
