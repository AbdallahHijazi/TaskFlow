using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Comment;
using TaskFlow.Application.Features.Comments.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Comments.Handlers;

public class GetAllCommentsQueryHandler : IRequestHandler<GetAllCommentsQuery, List<CommentDto>>
{
    private readonly IRepository<Comment> _repository;
    private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

    public GetAllCommentsQueryHandler(IRepository<Comment> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<List<CommentDto>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll();
        if (!_currentUser.IsAdmin)
        {
            var userId = _currentUser.UserId;
            query = query.Where(comment => userId.HasValue && comment.Task != null && comment.Task.AssignedToId == userId.Value);
        }
        return await query
            .AsNoTracking()
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content ?? string.Empty,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserName = c.User == null ? null : c.User.Name,
                TaskId = c.TaskId
            })
            .ToListAsync(cancellationToken);
    }
}
