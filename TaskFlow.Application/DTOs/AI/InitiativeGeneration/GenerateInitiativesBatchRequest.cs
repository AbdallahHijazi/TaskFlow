namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration;

public sealed class GenerateInitiativesBatchRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public Guid StatusId { get; set; }
    public Guid AssignedToId { get; set; }
}
