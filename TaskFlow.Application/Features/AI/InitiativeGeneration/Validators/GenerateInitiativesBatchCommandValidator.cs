using FluentValidation;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Validators;

public sealed class GenerateInitiativesBatchCommandValidator : AbstractValidator<GenerateInitiativesBatchCommand>
{
    public GenerateInitiativesBatchCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.Prompt)
            .NotEmpty().MinimumLength(10).MaximumLength(2000)
            .WithMessage("يرجى كتابة وصف واضح للمبادرات المطلوبة.");
        RuleFor(x => x.Request.Count)
            .InclusiveBetween(1, 4)
            .WithMessage("عدد المبادرات يجب أن يكون بين 1 و4.");
        RuleFor(x => x.Request.StatusId).NotEmpty().WithMessage("حالة المبادرة مطلوبة.");
        RuleFor(x => x.Request.AssignedToId).NotEmpty().WithMessage("المستخدم المسؤول عن المبادرة مطلوب.");
    }
}
