using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class GetTaskBoardQueryHandler : IRequestHandler<GetTaskBoardQuery, List<TaskBoardColumnDto>>
    {
        private const string UnknownStatusName = "Unknown Status";
        private readonly IRepository<TaskItem> _taskRepository;
        private readonly IRepository<Status> _statusRepository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public GetTaskBoardQueryHandler(
            IRepository<TaskItem> taskRepository,
            IRepository<Status> statusRepository,
            TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _taskRepository = taskRepository;
            _statusRepository = statusRepository;
            _currentUser = currentUser;
        }

        public async Task<List<TaskBoardColumnDto>> Handle(GetTaskBoardQuery request, CancellationToken cancellationToken)
        {
            var statuses = await _statusRepository.GetAll()
                .OrderBy(s => s.Name)
                .Select(s => new TaskBoardColumnDto
                {
                    StatusId = s.Id,
                    StatusName = string.IsNullOrWhiteSpace(s.Name) ? UnknownStatusName : s.Name,
                    Color = s.Color,
                    Tasks = new List<TaskDto>()
                })
                .ToListAsync(cancellationToken);

            var taskQuery = _taskRepository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                taskQuery = taskQuery.Where(t => userId.HasValue && t.AssignedToId == userId.Value);
            }

            var tasks = await taskQuery
                .OrderBy(t => t.EndDate)
                .ThenBy(t => t.Name)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Progress = t.Progress,
                    StatusId = t.StatusId,
                    StatusName = t.Status == null || string.IsNullOrWhiteSpace(t.Status.Name) ? UnknownStatusName : t.Status.Name,
                    InitiativeId = t.InitiativeId,
                    InitiativeName = t.Initiative == null ? null : t.Initiative.Name,
                    AssignedToId = t.AssignedToId,
                    AssignedToName = t.AssignedTo == null ? null : t.AssignedTo.Name,
                    Icon = t.Icon,
                    Color = t.Color,
                    CreatedById = t.CreatedBy ?? Guid.Empty,
                    ImageId = t.ImageId,
                    ImageUrl = t.ImageId == null ? null : $"/api/Images/{t.ImageId}/file",
                    ThumbnailUrl = t.ImageId == null ? null : $"/api/Images/{t.ImageId}/thumbnail",
                    FilePath = null,
                    ImageFileName = t.Image == null ? null : t.Image.FileName,
                    ImageContentType = t.Image == null ? null : t.Image.MediaType,
                    ImageSizeInBytes = t.Image == null ? null : t.Image.SizeInBytes,
                    UpdatedAt = t.UpdatedAt,
                    UpdatedById = t.UpdatedBy,
                    IsAISuggested = t.IsAISuggested
                })
                .ToListAsync(cancellationToken);

            var columns = statuses;
            var columnsByStatusId = columns
                .Where(column => column.StatusId.HasValue)
                .ToDictionary(column => column.StatusId!.Value);

            TaskBoardColumnDto? unknownColumn = null;

            foreach (var task in tasks)
            {
                if (task.StatusId.HasValue && columnsByStatusId.TryGetValue(task.StatusId.Value, out var column))
                {
                    column.Tasks.Add(task);
                    continue;
                }

                unknownColumn ??= new TaskBoardColumnDto
                {
                    StatusId = null,
                    StatusName = UnknownStatusName,
                    Color = null,
                    Tasks = new List<TaskDto>()
                };

                unknownColumn.Tasks.Add(task);
            }

            if (unknownColumn != null)
            {
                columns.Add(unknownColumn);
            }

            foreach (var column in columns)
            {
                column.TaskCount = column.Tasks.Count;
            }

            return columns;
        }
    }
}
