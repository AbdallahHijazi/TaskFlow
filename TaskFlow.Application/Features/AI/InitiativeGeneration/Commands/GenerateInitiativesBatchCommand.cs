using MediatR;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

public sealed record GenerateInitiativesBatchCommand(GenerateInitiativesBatchRequest Request)
    : IRequest<List<GeneratedInitiativePreview>>;
