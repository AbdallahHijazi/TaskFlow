using MediatR;
using TaskFlow.Application.DTOs.Comment;

namespace TaskFlow.Application.Features.Comments.Commands;

public class GetCommentByIdQuery : IRequest<CommentDto>
{
    public Guid Id { get; set; }

    public GetCommentByIdQuery(Guid id)
    {
        Id = id;
    }
}
