using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Statuses.Commands
{
    public class CreateStatusCommand : IRequest<StatusDto>
    {
        public CreateStatusDto Dto { get; set; }

        public CreateStatusCommand(CreateStatusDto dto)
        {
            Dto = dto;
        }
    }
}
