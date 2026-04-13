namespace TaskFlow.Application.Common.Models
{
    public sealed class ImageStorageSaveResult
    {
        public required string WebRelativePath { get; init; }
        public required string OriginalFileName { get; init; }
        public required long SizeInBytes { get; init; }
        public required string MediaType { get; init; }
    }
}
