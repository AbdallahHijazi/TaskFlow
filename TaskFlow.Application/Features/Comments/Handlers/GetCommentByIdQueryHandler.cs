using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Comment;
using TaskFlow.Application.Features.Comments.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Comments.Handlers;

public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, CommentDto>
{
    private readonly IRepository<Comment> _repository;

    public GetCommentByIdQueryHandler(IRepository<Comment> repository)
    {
        _repository = repository;
    }

    public async Task<CommentDto> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (comment == null)
            throw new NotFoundException("التعليق", request.Id);

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content ?? string.Empty,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            TaskId = comment.TaskId
        };
    }
}
