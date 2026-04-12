using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class DeleteImageCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteImageCommand(Guid id)
        {
            Id = id;
        }
    }
}
