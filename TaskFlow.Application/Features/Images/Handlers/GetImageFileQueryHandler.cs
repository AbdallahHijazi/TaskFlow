using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class GetImageFileQueryHandler : IRequestHandler<GetImageFileQuery, ImageFileStreamResult>
    {
        private readonly IRepository<Image> _repository;
        private readonly IImageFileStorage _fileStorage;

        public GetImageFileQueryHandler(IRepository<Image> repository, IImageFileStorage fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        public async Task<ImageFileStreamResult> Handle(GetImageFileQuery request, CancellationToken cancellationToken)
        {
            var image = await _repository.GetAll()
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
                .Select(i => new { i.FilePath, i.MediaType, i.FileName })
                .FirstOrDefaultAsync(cancellationToken);

            if (image == null || string.IsNullOrWhiteSpace(image.FilePath))
                throw new NotFoundException("الصورة", request.Id);

            var stream = await _fileStorage.OpenReadAsync(image.FilePath!, cancellationToken);
            if (stream == null)
                throw new NotFoundException("الصورة", request.Id);

            return new ImageFileStreamResult
            {
                Stream = stream,
                ContentType = string.IsNullOrWhiteSpace(image.MediaType) ? "application/octet-stream" : image.MediaType!,
                DownloadName = string.IsNullOrWhiteSpace(image.FileName) ? "image" : image.FileName!
            };
        }
    }
}
