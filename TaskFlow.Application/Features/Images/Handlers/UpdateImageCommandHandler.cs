using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Image;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class UpdateImageCommandHandler : IRequestHandler<UpdateImageCommand, ImageDto>
    {
        private readonly IRepository<Image> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageFileStorage _fileStorage;

        public UpdateImageCommandHandler(
            IRepository<Image> repository,
            IUnitOfWork unitOfWork,
            IImageFileStorage fileStorage)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<ImageDto> Handle(UpdateImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var image = await _repository.GetAll()
                    .Where(i => i.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (image == null)
                    throw new NotFoundException("الصورة", request.Id);

                var oldPath = image.FilePath;

                var stored = await _fileStorage.SaveAsync(
                    request.FileStream,
                    request.OriginalFileName,
                    request.ContentType,
                    cancellationToken);

                image.FileName = stored.OriginalFileName;
                image.FilePath = stored.WebRelativePath;
                image.MediaType = stored.MediaType;
                image.SizeInBytes = stored.SizeInBytes;
                image.UploadedAt = DateTime.UtcNow;

                try
                {
                    _repository.Update(image);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    await _fileStorage.DeleteAsync(stored.WebRelativePath, cancellationToken);
                    throw;
                }

                if (!string.IsNullOrWhiteSpace(oldPath))
                    await _fileStorage.DeleteAsync(oldPath, cancellationToken);

                return new ImageDto
                {
                    Id = image.Id,
                    FileName = image.FileName ?? string.Empty,
                    FilePath = image.FilePath ?? string.Empty,
                    MediaType = image.MediaType ?? string.Empty,
                    SizeInBytes = image.SizeInBytes ?? 0,
                    ThumbnailPath = image.ThumbnailPath,
                    UploadedAt = image.UploadedAt
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء تحديث الصورة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
