using FluentValidation;
using TaskFlow.Application.Features.TaskDependencies.Commands;

namespace TaskFlow.Application.Features.TaskDependencies.Validators;

public class UpdateTaskDependencyCommandValidator : AbstractValidator<UpdateTaskDependencyCommand>
{
    public UpdateTaskDependencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف تبعية المهمة مطلوب");

        RuleFor(x => x.Dto).NotNull().WithMessage("بيانات تبعية المهمة مطلوبة");
        RuleFor(x => x.Dto.DependencyTypeId)
            .NotNull().WithMessage("نوع التبعية مطلوب");
        RuleFor(x => x.Dto.PredecessorId)
            .NotNull().WithMessage("المهمة السابقة مطلوبة");
        RuleFor(x => x.Dto.SuccessorId)
            .NotNull().WithMessage("المهمة اللاحقة مطلوبة");

        RuleFor(x => x)
            .Must(x => x.Dto.PredecessorId != x.Dto.SuccessorId)
            .WithMessage("لا يمكن أن تكون المهمة السابقة واللاحقة نفس المهمة");
    }
}
