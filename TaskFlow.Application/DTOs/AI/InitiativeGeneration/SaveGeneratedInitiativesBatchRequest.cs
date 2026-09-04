namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration;

public sealed class SaveGeneratedInitiativesBatchRequest
{
    public List<SaveGeneratedInitiativeRequest> Initiatives { get; set; } = [];
}
