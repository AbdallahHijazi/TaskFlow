using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Image;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class CreateImageCommandHandler : IRequestHandler<CreateImageCommand, ImageDto>
    {
        private readonly IRepository<Image> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageFileStorage _fileStorage;

        public CreateImageCommandHandler(
            IRepository<Image> repository,
            IUnitOfWork unitOfWork,
            IImageFileStorage fileStorage)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<ImageDto> Handle(CreateImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var stored = await _fileStorage.SaveAsync(
                    request.FileStream,
                    request.OriginalFileName,
                    request.ContentType,
                    cancellationToken);

                var image = new Image
                {
                    FileName = stored.OriginalFileName,
                    FilePath = stored.WebRelativePath,
                    MediaType = stored.MediaType,
                    SizeInBytes = stored.SizeInBytes,
                    UploadedAt = DateTime.UtcNow,
                    ThumbnailPath = null
                };

                try
                {
                    _repository.Add(image);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    await _fileStorage.DeleteAsync(stored.WebRelativePath, cancellationToken);
                    throw;
                }

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
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء رفع الصورة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
