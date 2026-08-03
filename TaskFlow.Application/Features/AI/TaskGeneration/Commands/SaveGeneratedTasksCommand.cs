using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.TaskGeneration;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Commands
{
    public sealed class SaveGeneratedTasksCommand
        : IRequest<SaveGeneratedTasksResponse>
    {
        public SaveGeneratedTasksRequest Request { get; }

        public SaveGeneratedTasksCommand(
            SaveGeneratedTasksRequest request)
        {
            Request = request;
        }
    }
}
