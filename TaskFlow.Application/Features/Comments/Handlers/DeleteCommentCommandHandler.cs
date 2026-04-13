using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Comments.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Comments.Handlers;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IRepository<Comment> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IRepository<Comment> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetAll().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (comment == null)
            throw new NotFoundException("التعليق", request.Id);

        _repository.Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
