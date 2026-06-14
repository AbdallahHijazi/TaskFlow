using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface IImageService
    {
        Task<Guid?> SaveImageAsync(IFormFile? file, CancellationToken cancellationToken);
    }
}
