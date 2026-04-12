using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Image;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class CreateImageCommand : IRequest<ImageDto>
    {
        public CreateImageDto Dto { get; set; }

        public CreateImageCommand(CreateImageDto dto)
        {
            Dto = dto;
        }
    }
}
