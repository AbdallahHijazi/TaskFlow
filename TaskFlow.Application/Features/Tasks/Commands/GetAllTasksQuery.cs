using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class GetAllTasksQuery : IRequest<PagedResultDto<TaskDto>>
    {
        public Guid? AssignedToId { get; }
        public Guid? StatusId { get; }
        public Guid? InitiativeId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public string? Search { get; }

        public GetAllTasksQuery(Guid? assignedToId = null, Guid? statusId = null, Guid? initiativeId = null, int pageNumber = 1, int pageSize = 20, string? search = null)
        {
            AssignedToId = assignedToId;
            StatusId = statusId;
            InitiativeId = initiativeId;
            PageNumber = Math.Max(1, pageNumber);
            PageSize = Math.Clamp(pageSize, 1, 100);
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        }
    }
}
