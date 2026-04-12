using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Image;
using TaskFlow.Application.Features.Images.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Images.Handlers
{
    public class CreateImageCommandHandler : IRequestHandler<CreateImageCommand, ImageDto>
    {
        private readonly IRepository<Image> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateImageCommandHandler(IRepository<Image> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ImageDto> Handle(CreateImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var image = new Image
                {
                    FileName = request.Dto.FileName,
                    FilePath = request.Dto.FilePath,
                    MediaType = request.Dto.MediaType,
                    SizeInBytes = request.Dto.SizeInBytes,
                    ThumbnailPath = request.Dto.ThumbnailPath
                };

                _repository.Add(image);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ImageDto
                {
                    Id = image.Id,
                    FileName = image.FileName,
                    FilePath = image.FilePath,
                    MediaType = image.MediaType,
                    SizeInBytes = image.SizeInBytes ?? 0,
                    ThumbnailPath = image.ThumbnailPath
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء رفع الصورة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
