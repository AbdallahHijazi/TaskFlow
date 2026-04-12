using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class DeleteTaskCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteTaskCommand(Guid id)
        {
            Id = id;
        }
    }
}
