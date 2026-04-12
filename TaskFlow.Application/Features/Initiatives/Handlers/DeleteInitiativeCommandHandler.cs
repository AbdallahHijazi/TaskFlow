using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Initiatives.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class DeleteInitiativeCommandHandler : IRequestHandler<DeleteInitiativeCommand, bool>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteInitiativeCommandHandler(IRepository<Initiative> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteInitiativeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var initiative = await _repository.GetAll()
                    .Where(i => i.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (initiative == null)
                    throw new NotFoundException("المبادرة", request.Id);

                _repository.Delete(initiative);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء حذف المبادرة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
