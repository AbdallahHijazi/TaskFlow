using MediatR;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class GetImageFileQuery : IRequest<ImageFileStreamResult>
    {
        public Guid Id { get; }

        public GetImageFileQuery(Guid id)
        {
            Id = id;
        }
    }
}
