using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, List<TaskDto>>
    {
        private readonly IRepository<TaskItem> _repository;

        public GetAllTasksQueryHandler(IRepository<TaskItem> repository)
        {
            _repository = repository;
        }

        public async Task<List<TaskDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _repository.GetAll()
                .AsNoTracking()
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Progress = t.Progress,
                    StatusId = t.StatusId,
                    InitiativeId = t.InitiativeId,
                    AssignedToId = t.AssignedToId,
                    CreatedById = t.CreatedBy ?? Guid.Empty,
                    ImageId = t.ImageId
                })
                .ToListAsync(cancellationToken);

            return tasks;
        }
    }
}
