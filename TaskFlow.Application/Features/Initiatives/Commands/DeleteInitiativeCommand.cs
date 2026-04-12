using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Features.Initiatives.Commands
{
    public class DeleteInitiativeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteInitiativeCommand(Guid id)
        {
            Id = id;
        }
    }
}
