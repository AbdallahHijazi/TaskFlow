using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Statuses.Handlers
{
    public class DeleteStatusCommandHandler : IRequestHandler<DeleteStatusCommand, bool>
    {
        private readonly IRepository<Status> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStatusCommandHandler(IRepository<Status> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var status = _repository.Get(request.Id);

                if (status == null)
                {
                    throw new NotFoundException("الحالة", request.Id);
                }

                _repository.Delete(status);
                await _unitOfWork.SaveChangesAsync();

                return true;   
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء حذف الحالة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }
    }
}
