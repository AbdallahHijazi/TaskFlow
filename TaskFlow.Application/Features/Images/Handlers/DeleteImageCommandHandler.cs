using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, bool>
    {
        private readonly IRepository<Image> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteImageCommandHandler(IRepository<Image> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var image = await _repository.GetAll()
                    .Where(i => i.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (image == null)
                    throw new NotFoundException("الصورة", request.Id);

                _repository.Delete(image);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء حذف الصورة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
