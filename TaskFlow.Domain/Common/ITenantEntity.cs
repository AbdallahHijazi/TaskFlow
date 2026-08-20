namespace TaskFlow.Domain.Common;

public interface ITenantEntity
{
    Guid ClientId { get; set; }
}
