using MediatR;
using TaskFlow.Application.DTOs.Client;
using TaskFlow.Application.DTOs.User;
namespace TaskFlow.Application.Features.Clients.Commands;
public record RegisterClientCommand(RegisterClientDto Dto) : IRequest<UserDto>;
