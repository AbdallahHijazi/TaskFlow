using FluentValidation;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Validators;

public sealed class GenerateInitiativesBatchCommandValidator : AbstractValidator<GenerateInitiativesBatchCommand>
{
    public GenerateInitiativesBatchCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.Prompt)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(2000)
            .WithMessage("Please provide a clear description for the requested initiatives.");
        RuleFor(x => x.Request.Count)
            .InclusiveBetween(1, 4)
            .WithMessage("The initiative count must be between 1 and 4.");
    }
}
