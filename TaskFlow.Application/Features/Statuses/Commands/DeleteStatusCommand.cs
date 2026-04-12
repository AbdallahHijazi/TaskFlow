using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Features.Statuses.Commands
{
    public class DeleteStatusCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteStatusCommand(Guid id)
        {
            Id = id;
        }
    }
}
