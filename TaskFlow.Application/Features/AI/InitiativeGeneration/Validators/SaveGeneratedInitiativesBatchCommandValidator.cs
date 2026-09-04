using FluentValidation;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Validators;

public sealed class SaveGeneratedInitiativesBatchCommandValidator
    : AbstractValidator<SaveGeneratedInitiativesBatchCommand>
{
    public SaveGeneratedInitiativesBatchCommandValidator()
    {
        RuleFor(command => command.Request).NotNull().WithMessage("بيانات الحفظ مطلوبة.");
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.Initiatives)
                .NotEmpty().WithMessage("يجب اختيار مبادرة واحدة على الأقل.")
                .Must(items => items is { Count: <= 20 }).WithMessage("لا يمكن حفظ أكثر من 20 مبادرة في دفعة واحدة.");

            RuleForEach(command => command.Request.Initiatives)
                .SetValidator(command => new SaveGeneratedInitiativeRequestValidator());

            RuleFor(command => command.Request.Initiatives)
                .Must(items => items is not null && items.Select(item => item.Name?.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count)
                .WithMessage("يجب ألا تتكرر أسماء المبادرات في الدفعة.");
        });
    }

    private sealed class SaveGeneratedInitiativeRequestValidator
        : AbstractValidator<DTOs.AI.InitiativeGeneration.SaveGeneratedInitiativeRequest>
    {
        public SaveGeneratedInitiativeRequestValidator()
        {
            RuleFor(item => item.Name).NotEmpty().MaximumLength(200);
            RuleFor(item => item.Description).NotEmpty().MaximumLength(2000);
            RuleFor(item => item.EndDate).NotNull().GreaterThanOrEqualTo(item => item.StartDate);
            RuleFor(item => item.Color).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$");
            RuleFor(item => item.Icon).NotEmpty();
            RuleFor(item => item.Tasks).NotEmpty();
            RuleForEach(item => item.Tasks).ChildRules(task =>
            {
                task.RuleFor(value => value.Name).NotEmpty().MaximumLength(200);
                task.RuleFor(value => value.Description).NotEmpty().MaximumLength(2000);
                task.RuleFor(value => value.EndDate).NotNull().GreaterThanOrEqualTo(value => value.StartDate);
                task.RuleFor(value => value.Color).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$");
                task.RuleFor(value => value.Icon).NotEmpty();
            });
            RuleFor(item => item).Must(item => item.Tasks.All(task =>
                task.StartDate >= item.StartDate && task.EndDate is not null && task.EndDate <= item.EndDate));
        }
    }
}
