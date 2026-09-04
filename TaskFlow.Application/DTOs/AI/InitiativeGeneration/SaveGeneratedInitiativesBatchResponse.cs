namespace TaskFlow.Application.DTOs.AI.InitiativeGeneration;

public sealed class SaveGeneratedInitiativesBatchResponse
{
    public List<Guid> InitiativeIds { get; set; } = [];
    public int CreatedInitiativesCount { get; set; }
    public int CreatedTasksCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
