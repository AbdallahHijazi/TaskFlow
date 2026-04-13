using MediatR;
using TaskFlow.Application.DTOs.Comment;

namespace TaskFlow.Application.Features.Comments.Commands;

public class UpdateCommentCommand : IRequest<CommentDto>
{
    public Guid Id { get; set; }
    public CreateCommentDto Dto { get; set; }

    public UpdateCommentCommand(Guid id, CreateCommentDto dto)
    {
        Id = id;
        Dto = dto;
    }
}
