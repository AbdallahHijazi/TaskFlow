using System.IO;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface IImageFileStorage
    {
        Task<ImageStorageSaveResult> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default);

        Task DeleteAsync(string? webRelativePathUnderWwwroot, CancellationToken cancellationToken = default);

        Task<Stream?> OpenReadAsync(string webRelativePathUnderWwwroot, CancellationToken cancellationToken = default);
    }
}
