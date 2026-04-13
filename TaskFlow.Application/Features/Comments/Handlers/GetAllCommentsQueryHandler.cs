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

    public GetAllCommentsQueryHandler(IRepository<Comment> repository)
    {
        _repository = repository;
    }

    public async Task<List<CommentDto>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAll()
            .AsNoTracking()
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content ?? string.Empty,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                TaskId = c.TaskId
            })
            .ToListAsync(cancellationToken);
    }
}
