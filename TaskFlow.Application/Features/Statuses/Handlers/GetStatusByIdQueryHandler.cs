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
    public class GetStatusByIdQueryHandler : IRequestHandler<GetStatusByIdQuery, StatusDto>
    {
        private readonly IRepository<Status> _repository;

        public GetStatusByIdQueryHandler(IRepository<Status> repository)
        {
            _repository = repository;
        }

        public async Task<StatusDto> Handle(GetStatusByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var status = await _repository.GetAll()
                    .AsNoTracking()
                    .Where(s => s.Id == request.Id)
                    .Select(s => new StatusDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Color = s.Color
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (status == null)
                {
                    throw new NotFoundException("الحالة", request.Id);
                }

                return status;
            }
            catch (NotFoundException)
            {
                throw;   
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء جلب الحالة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }
    }
}
