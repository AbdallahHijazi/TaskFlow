using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Image;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class GetImageByIdQueryHandler : IRequestHandler<GetImageByIdQuery, ImageDto>
    {
        private readonly IRepository<Image> _repository;

        public GetImageByIdQueryHandler(IRepository<Image> repository)
        {
            _repository = repository;
        }

        public async Task<ImageDto> Handle(GetImageByIdQuery request, CancellationToken cancellationToken)
        {
            var image = await _repository.GetAll()
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
                .Select(i => new ImageDto
                {
                    Id = i.Id,
                    FileName = i.FileName ?? string.Empty,
                    FilePath = i.FilePath ?? string.Empty,
                    MediaType = i.MediaType ?? string.Empty,
                    SizeInBytes = i.SizeInBytes ?? 0,
                    ThumbnailPath = i.ThumbnailPath,
                    UploadedAt = i.UploadedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (image == null)
                throw new NotFoundException("الصورة", request.Id);

            return image;
        }
    }
}
