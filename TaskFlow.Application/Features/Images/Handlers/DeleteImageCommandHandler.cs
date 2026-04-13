using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IImageFileStorage _fileStorage;

        public DeleteImageCommandHandler(
            IRepository<Image> repository,
            IUnitOfWork unitOfWork,
            IImageFileStorage fileStorage)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
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

                var path = image.FilePath;

                _repository.Delete(image);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _fileStorage.DeleteAsync(path, cancellationToken);

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
