using System.IO;

namespace TaskFlow.Application.Common.Models
{
    public sealed class ImageFileStreamResult
    {
        public required Stream Stream { get; init; }
        public required string ContentType { get; init; }
        public required string DownloadName { get; init; }
    }
}
