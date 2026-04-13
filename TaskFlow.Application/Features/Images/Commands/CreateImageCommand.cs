using System.IO;
using MediatR;
using TaskFlow.Application.DTOs.Image;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class CreateImageCommand : IRequest<ImageDto>
    {
        public Stream FileStream { get; }
        public string OriginalFileName { get; }
        public string ContentType { get; }

        public CreateImageCommand(Stream fileStream, string originalFileName, string contentType)
        {
            FileStream = fileStream;
            OriginalFileName = originalFileName;
            ContentType = contentType;
        }
    }
}
