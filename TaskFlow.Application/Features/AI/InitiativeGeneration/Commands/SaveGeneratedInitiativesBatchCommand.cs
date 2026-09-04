using MediatR;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

public sealed record SaveGeneratedInitiativesBatchCommand(SaveGeneratedInitiativesBatchRequest Request)
    : IRequest<SaveGeneratedInitiativesBatchResponse>;
