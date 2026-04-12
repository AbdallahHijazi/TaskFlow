using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTaskCommandHandler(IRepository<TaskItem> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var task = await _repository.GetAll()
                    .Where(t => t.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (task == null)
                    throw new NotFoundException("المهمة", request.Id);

                _repository.Delete(task);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء حذف المهمة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
