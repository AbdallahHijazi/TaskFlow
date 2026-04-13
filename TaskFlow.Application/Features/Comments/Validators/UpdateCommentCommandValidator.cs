using FluentValidation;
using TaskFlow.Application.Features.Comments.Commands;

namespace TaskFlow.Application.Features.Comments.Validators;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف التعليق مطلوب");

        RuleFor(x => x.Dto).NotNull().WithMessage("بيانات التعليق مطلوبة");
        RuleFor(x => x.Dto.Content)
            .NotEmpty().WithMessage("محتوى التعليق مطلوب")
            .MaximumLength(1000).WithMessage("محتوى التعليق يجب ألا يتجاوز 1000 حرف");
        RuleFor(x => x.Dto.UserId)
            .NotNull().WithMessage("معرف المستخدم مطلوب");
        RuleFor(x => x.Dto.TaskId)
            .NotNull().WithMessage("معرف المهمة مطلوب");
    }
}
