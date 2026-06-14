using MediatR;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class GetImageFileQuery : IRequest<ImageFileStreamResult>
    {
        public Guid Id { get; }
        public bool PreferThumbnail { get; }

        public GetImageFileQuery(Guid id, bool preferThumbnail = false)
        {
            Id = id;
            PreferThumbnail = preferThumbnail;
        }
    }
}
