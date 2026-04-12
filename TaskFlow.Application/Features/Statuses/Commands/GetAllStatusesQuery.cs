using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Status;

namespace TaskFlow.Application.Features.Statuses.Commands
{
    public class GetAllStatusesQuery : IRequest<List<StatusDto>>
    {
    }
}
