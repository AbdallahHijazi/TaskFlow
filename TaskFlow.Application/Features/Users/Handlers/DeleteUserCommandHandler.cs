using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Users.Handlers
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IRepository<User> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService currentUser;

        public DeleteUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            this.currentUser = currentUser;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var clientId = currentUser.ClientId ?? throw new TaskFlow.Domain.Exceptions.UnauthorizedException("Your session does not contain a client. Please sign in again.");
                var user = await _repository.GetAll()
                    .Where(u => u.Id == request.Id && u.ClientId == clientId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    throw new NotFoundException("المستخدم", request.Id);

                _repository.Delete(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (TaskFlow.Domain.Exceptions.UnauthorizedException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء حذف المستخدم. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
