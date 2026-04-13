using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Image;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class GetAllImagesQueryHandler : IRequestHandler<GetAllImagesQuery, List<ImageDto>>
    {
        private readonly IRepository<Image> _repository;

        public GetAllImagesQueryHandler(IRepository<Image> repository)
        {
            _repository = repository;
        }

        public async Task<List<ImageDto>> Handle(GetAllImagesQuery request, CancellationToken cancellationToken)
        {
            var images = await _repository.GetAll()
                .AsNoTracking()
                .Select(image => new ImageDto
                {
                    Id = image.Id,
                    FileName = image.FileName ?? string.Empty,
                    FilePath = image.FilePath ?? string.Empty,
                    MediaType = image.MediaType ?? string.Empty,
                    SizeInBytes = image.SizeInBytes ?? 0,
                    ThumbnailPath = image.ThumbnailPath,
                    UploadedAt = image.UploadedAt
                })
                .ToListAsync(cancellationToken);

            return images;
        }
    }
}
