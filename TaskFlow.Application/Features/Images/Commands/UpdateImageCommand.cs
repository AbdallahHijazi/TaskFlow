using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Image;

namespace TaskFlow.Application.Features.Images.Commands
{
    public class UpdateImageCommand : IRequest<ImageDto>
    {
        public Guid Id { get; set; }
        public CreateImageDto Dto { get; set; }   

        public UpdateImageCommand(Guid id, CreateImageDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
