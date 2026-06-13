using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Lookup;

namespace TaskFlow.Application.Features.Lookups.Commands
{
    public class GetUserLookupsQuery : IRequest<List<LookupItemDto>>
    {
    }
}
