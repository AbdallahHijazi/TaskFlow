using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageFileStorage _imageFileStorage;
        private readonly IRepository<Image> _imageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ImageService(
            IImageFileStorage imageFileStorage,
            IRepository<Image> imageRepository,
            IUnitOfWork unitOfWork)
        {
            _imageFileStorage = imageFileStorage;
            _imageRepository = imageRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Guid?> SaveImageAsync(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();

            var saved = await _imageFileStorage.SaveAsync(
                stream,
                file.FileName,
                file.ContentType,
                cancellationToken);

            var image = new Image
            {
                FileName = file.FileName,
                FilePath = saved.WebRelativePath,
                MediaType = file.ContentType,
                SizeInBytes = file.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedById = null
            };

            _imageRepository.Add(image);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return image.Id;
        }
    }
}
