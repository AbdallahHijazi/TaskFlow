using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Statuses.Handlers
{
    public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommand, StatusDto>
    {
        private readonly IRepository<Status> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStatusCommandHandler(IRepository<Status> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StatusDto> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var status = await _repository.GetAll()
                    .Where(s => s.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (status == null)
                {
                    throw new NotFoundException("الحالة", request.Id);
                }

                if (string.IsNullOrWhiteSpace(request.Dto.Name))
                {
                    throw new InvalidOperationException("اسم الحالة مطلوب ولا يمكن أن يكون فارغاً");
                }

                status.Name = request.Dto.Name.Trim();
                status.Description = request.Dto.Description?.Trim();
                status.Color = request.Dto.Color?.Trim();

                _repository.Update(status);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new StatusDto
                {
                    Id = status.Id,
                    Name = status.Name,
                    Description = status.Description,
                    Color = status.Color
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء تحديث الحالة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }
    }
}
