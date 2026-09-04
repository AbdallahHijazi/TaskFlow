
namespace TaskFlow.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Guid? ClientId { get; }
        bool IsAdmin => false;
    }
}
