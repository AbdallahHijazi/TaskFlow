using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public UpdateImageCommandHandler(IRepository<Image> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
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

                image.FileName = request.Dto.FileName;
                image.FilePath = request.Dto.FilePath;
                image.MediaType = request.Dto.MediaType;
                image.SizeInBytes = request.Dto.SizeInBytes;
                image.ThumbnailPath = request.Dto.ThumbnailPath;

                _repository.Update(image);
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
            catch (NotFoundException)
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
