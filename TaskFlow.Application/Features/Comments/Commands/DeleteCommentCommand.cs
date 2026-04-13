using MediatR;

namespace TaskFlow.Application.Features.Comments.Commands;

public class DeleteCommentCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteCommentCommand(Guid id)
    {
        Id = id;
    }
}
