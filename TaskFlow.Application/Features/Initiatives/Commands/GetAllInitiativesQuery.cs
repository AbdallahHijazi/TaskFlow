using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Initiatives.Commands
{
    public class GetAllInitiativesQuery : IRequest<PagedResultDto<InitiativeDto>>
    {
        public Guid? AssignedToId { get; }
        public Guid? StatusId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public string? Search { get; }

        public GetAllInitiativesQuery(Guid? assignedToId = null, Guid? statusId = null, int pageNumber = 1, int pageSize = 20, string? search = null)
        {
            AssignedToId = assignedToId;
            StatusId = statusId;
            PageNumber = Math.Max(1, pageNumber);
            PageSize = Math.Clamp(pageSize, 1, 100);
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        }
    }
}
