using System.IO;
using MediatR;
using TaskFlow.Application.DTOs.Image;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class UpdateImageCommand : IRequest<ImageDto>
    {
        public Guid Id { get; }
        public Stream FileStream { get; }
        public string OriginalFileName { get; }
        public string ContentType { get; }

        public UpdateImageCommand(Guid id, Stream fileStream, string originalFileName, string contentType)
        {
            Id = id;
            FileStream = fileStream;
            OriginalFileName = originalFileName;
            ContentType = contentType;
        }
    }
}
