using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Clients.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Common.Security;
namespace TaskFlow.Application.Features.Clients.Handlers;
public class RegisterClientCommandHandler : IRequestHandler<RegisterClientCommand, UserDto>
{
    private readonly IRepository<Client> clients; 
    private readonly IRepository<User> users;
    private readonly IRepository<Role> roles; 
    private readonly IRepository<Status> statuses;
    private readonly IRepository<DependencyType> dependencyTypes;
    private readonly IUnitOfWork unitOfWork; 
    private readonly IUserPasswordHasher passwordHasher;
    public RegisterClientCommandHandler(IRepository<Client> clients, IRepository<User> users, IRepository<Role> roles, IRepository<Status> statuses, IRepository<DependencyType> dependencyTypes, IUnitOfWork unitOfWork, IUserPasswordHasher passwordHasher) { this.clients=clients; this.users=users; this.roles=roles; this.statuses=statuses; this.dependencyTypes=dependencyTypes; this.unitOfWork=unitOfWork; this.passwordHasher=passwordHasher; }
    public async Task<UserDto> Handle(RegisterClientCommand request, CancellationToken ct)
    {
        var dto=request.Dto;
        var clientName=dto.ClientName.Trim();
        var email=EmailAddressPolicy.NormalizeAndValidate(dto.Email);
        if (clientName.Length<2) throw new InvalidOperationException("Client name is required.");
        if (string.IsNullOrWhiteSpace(dto.AdminName)) throw new InvalidOperationException("Administrator name is required.");
        if (dto.Password!=dto.ConfirmPassword) throw new InvalidOperationException("Password and confirmation do not match.");
        if (await clients.GetAll().AnyAsync(c=>c.Name.ToLower()==clientName.ToLower(),ct)) throw new InvalidOperationException("A client with this name already exists.");
        if (await users.GetAll().AnyAsync(u=>u.Email!=null&&u.Email.ToLower()==email,ct)) throw new InvalidOperationException("This email address is already registered.");
        var role = await roles.GetAll()
            .FirstOrDefaultAsync(r =>
                (r.RoleCode != null && r.RoleCode.ToLower() == "admin") ||
                (r.RoleName != null && r.RoleName.ToLower() == "admin"), ct);

        // A fresh or deliberately cleared database may not contain reference roles yet.
        // Client registration must still be able to create its first administrator.
        if (role is null)
        {
            role = new Role
            {
                RoleName = "Admin",
                RoleCode = "ADMIN"
            };
            roles.Add(role);
        }
        var client=new Client { Name=clientName, ContactEmail=email, IsActive=true };
        var user=new User { ClientId=client.Id, Client=client, Name=dto.AdminName.Trim(), Email=email, PhoneNumber=dto.PhoneNumber.Trim(), Password=passwordHasher.HashPassword(dto.Password), RoleId=role.RoleId };
        statuses.Add(new Status { ClientId=client.Id, Name="New", Description="Newly created work that has not started yet.", Color="#94A3B8" });
        statuses.Add(new Status { ClientId=client.Id, Name="Planned", Description="Work that has not started yet.", Color="#64748B" });
        statuses.Add(new Status { ClientId=client.Id, Name="In Progress", Description="Work currently in progress.", Color="#2563EB" });
        statuses.Add(new Status { ClientId=client.Id, Name="At Risk", Description="Work requiring attention.", Color="#DC2626" });
        statuses.Add(new Status { ClientId=client.Id, Name="Completed", Description="Work completed successfully.", Color="#059669" });
        dependencyTypes.Add(new DependencyType { ClientId=client.Id, Name="Finish to Start", Description="The predecessor must finish before the successor can start." });
        dependencyTypes.Add(new DependencyType { ClientId=client.Id, Name="Start to Start", Description="Both tasks start together." });
        dependencyTypes.Add(new DependencyType { ClientId=client.Id, Name="Finish to Finish", Description="Both tasks finish together." });
        dependencyTypes.Add(new DependencyType { ClientId=client.Id, Name="Start to Finish", Description="The successor cannot finish until the predecessor starts." });
        clients.Add(client); users.Add(user); await unitOfWork.SaveChangesAsync(ct);
        return new UserDto { Id=user.Id,Name=user.Name,Email=user.Email,PhoneNumber=user.PhoneNumber,RoleId=role.RoleId,RoleName=role.RoleName,ClientId=client.Id,ClientName=client.Name,CreatedAt=user.CreatedAt };
    }
}
