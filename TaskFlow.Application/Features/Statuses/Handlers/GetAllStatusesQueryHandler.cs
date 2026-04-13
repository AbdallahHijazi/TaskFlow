using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Statuses.Handlers
{
    public class GetAllStatusesQueryHandler : IRequestHandler<GetAllStatusesQuery, List<StatusDto>>
    {
        private readonly IRepository<Status> _repository;

        public GetAllStatusesQueryHandler(IRepository<Status> repository)
        {
            _repository = repository;
        }

        public async Task<List<StatusDto>> Handle(GetAllStatusesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var statuses = await _repository.GetAll()
                    .Select(s => new StatusDto
                    {
                        Id = s.Id,
                        Name = s.Name ?? string.Empty,
                        Description = s.Description,
                        Color = s.Color
                    })
                    .ToListAsync(cancellationToken);

                if (statuses == null || statuses.Count == 0)
                {
                    throw new InvalidOperationException("لا توجد حالات مسجلة حالياً");
                }

                return statuses.Select(s => new StatusDto
                {
                    Id = s.Id,
                    Name = s.Name ?? string.Empty,
                    Description = s.Description,
                    Color = s.Color
                }).ToList();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء جلب قائمة الحالات. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }
    }
}
