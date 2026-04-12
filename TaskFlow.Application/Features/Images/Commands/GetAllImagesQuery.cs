using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Image;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class GetAllImagesQuery : IRequest<List<ImageDto>>
    {
    }
}
