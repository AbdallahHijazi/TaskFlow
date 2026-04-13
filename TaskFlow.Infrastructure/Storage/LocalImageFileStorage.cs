using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Infrastructure.Storage
{
    public class LocalImageFileStorage : IImageFileStorage
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly string _relativeFolder;
        private readonly long _maxBytes;
        private readonly HashSet<string> _allowedExt;

        public LocalImageFileStorage(IHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _hostEnvironment = hostEnvironment;
            _relativeFolder = (configuration["ImageStorage:FolderRelativeToWwwRoot"] ?? "uploads/images")
                .Trim()
                .TrimStart('/')
                .Replace('\\', '/');
            _maxBytes = long.TryParse(configuration["ImageStorage:MaxBytes"], out var m) ? m : 10L * 1024 * 1024;
            var extList = configuration["ImageStorage:AllowedExtensions"] ?? ".jpg,.jpeg,.png,.gif,.webp,.bmp";
            _allowedExt = new HashSet<string>(
                extList.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : "." + e.ToLowerInvariant()));
        }

        private string WwwRootPath => Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot");

        public async Task<ImageStorageSaveResult> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            var safeName = Path.GetFileName(originalFileName);
            if (string.IsNullOrWhiteSpace(safeName))
                throw new BadRequestException("اسم الملف غير صالح");

            var ext = Path.GetExtension(safeName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_allowedExt.Contains(ext))
                throw new BadRequestException("صيغة الملف غير مدعومة");

            var storedFileName = $"{Guid.NewGuid():N}{ext}";
            var webRelativePath = $"{_relativeFolder}/{storedFileName}".Replace('\\', '/');

            var directoryPhysical = Path.Combine(WwwRootPath, _relativeFolder.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(directoryPhysical);
            var physicalPath = Path.Combine(directoryPhysical, storedFileName);

            long written = 0;
            await using (var target = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                {
                    written += read;
                    if (written > _maxBytes)
                        throw new BadRequestException("حجم الملف يتجاوز الحد المسموح");

                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
            }

            if (written == 0)
            {
                TryDeletePhysical(physicalPath);
                throw new BadRequestException("الملف فارغ");
            }

            return new ImageStorageSaveResult
            {
                WebRelativePath = webRelativePath,
                OriginalFileName = safeName,
                SizeInBytes = written,
                MediaType = NormalizeContentType(contentType, ext)
            };
        }

        private static void TryDeletePhysical(string physicalPath)
        {
            try
            {
                if (File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
            catch
            {
                // ignore
            }
        }

        private static string NormalizeContentType(string contentType, string extension)
        {
            if (!string.IsNullOrWhiteSpace(contentType))
                return contentType;

            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }

        public Task DeleteAsync(string? webRelativePathUnderWwwroot, CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            if (string.IsNullOrWhiteSpace(webRelativePathUnderWwwroot))
                return Task.CompletedTask;

            try
            {
                var full = ResolvePhysicalUnderWwwRoot(webRelativePathUnderWwwroot);
                if (full != null && File.Exists(full))
                    File.Delete(full);
            }
            catch
            {
                // ignore disk delete failures
            }

            return Task.CompletedTask;
        }

        public Task<Stream?> OpenReadAsync(string webRelativePathUnderWwwroot, CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            if (string.IsNullOrWhiteSpace(webRelativePathUnderWwwroot))
                return Task.FromResult<Stream?>(null);

            try
            {
                var full = ResolvePhysicalUnderWwwRoot(webRelativePathUnderWwwroot);
                if (full == null || !File.Exists(full))
                    return Task.FromResult<Stream?>(null);

                Stream stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                return Task.FromResult<Stream?>(stream);
            }
            catch
            {
                return Task.FromResult<Stream?>(null);
            }
        }

        private string? ResolvePhysicalUnderWwwRoot(string webRelativePathUnderWwwroot)
        {
            var trimmed = webRelativePathUnderWwwroot.Trim().TrimStart('/').Replace('\\', '/');
            var combined = Path.GetFullPath(Path.Combine(WwwRootPath, trimmed.Replace('/', Path.DirectorySeparatorChar)));
            var rootFull = Path.GetFullPath(WwwRootPath);
            if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                return null;
            return combined;
        }
    }
}
