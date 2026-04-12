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

        public DeleteUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _repository.GetAll()
                    .Where(u => u.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    throw new NotFoundException("المستخدم", request.Id);

                _repository.Delete(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
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
