using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Status;

namespace TaskFlow.Application.Features.Statuses.Commands
{
    public class GetStatusByIdQuery : IRequest<StatusDto>
    {
        public Guid Id { get; set; }
        public GetStatusByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
