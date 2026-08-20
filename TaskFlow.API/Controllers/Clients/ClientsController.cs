using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Client;
using TaskFlow.Application.Features.Clients.Commands;
namespace TaskFlow.API.Controllers.Clients;
[ApiController,Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IMediator mediator; public ClientsController(IMediator mediator)=>this.mediator=mediator;
    [AllowAnonymous,HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterClientDto dto,CancellationToken ct)=>Created(string.Empty,await mediator.Send(new RegisterClientCommand(dto),ct));
}
