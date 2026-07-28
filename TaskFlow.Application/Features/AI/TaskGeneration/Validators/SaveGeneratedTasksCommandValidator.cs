using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Features.AI.TaskGeneration.Commands;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Validators
{
    public sealed class SaveGeneratedTasksCommandValidator
        : AbstractValidator<SaveGeneratedTasksCommand>
    {
        public SaveGeneratedTasksCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("بيانات حفظ المهام مطلوبة.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.InitiativeId)
                    .NotEmpty()
                    .WithMessage("معرّف المبادرة مطلوب.");

                RuleFor(x => x.Request.StatusId)
                    .NotEmpty()
                    .WithMessage("معرّف حالة المهمة مطلوب.");

                RuleFor(x => x.Request.AssignedToId)
                    .NotEmpty()
                    .WithMessage("معرّف المستخدم المسؤول مطلوب.");

                RuleFor(x => x.Request.Tasks)
                    .NotNull()
                    .WithMessage("قائمة المهام مطلوبة.")
                    .NotEmpty()
                    .WithMessage("يجب إرسال مهمة واحدة على الأقل.")
                    .Must(tasks => tasks is not null && tasks.Count <= 4)
                    .WithMessage("لا يمكن حفظ أكثر من 4 مهام في الطلب الواحد.");

                RuleForEach(x => x.Request.Tasks)
                    .ChildRules(task =>
                    {
                        task.RuleFor(x => x.Name)
                            .NotEmpty()
                            .WithMessage("اسم المهمة مطلوب.")
                            .MaximumLength(200)
                            .WithMessage("اسم المهمة طويل جدًا.");

                        task.RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage("وصف المهمة مطلوب.");

                        task.RuleFor(x => x.StartDate)
                            .NotEmpty()
                            .WithMessage("تاريخ بداية المهمة مطلوب.");

                        task.RuleFor(x => x.EndDate)
                            .NotEmpty()
                            .WithMessage("تاريخ نهاية المهمة مطلوب.")
                            .GreaterThanOrEqualTo(x => x.StartDate)
                            .WithMessage("تاريخ نهاية المهمة يجب ألا يسبق تاريخ بدايتها.");

                        task.RuleFor(x => x.Color)
                            .NotEmpty()
                            .WithMessage("لون المهمة مطلوب.")
                            .Matches("^#[0-9A-Fa-f]{6}$")
                            .WithMessage("لون المهمة يجب أن يكون بصيغة Hex مثل #4F46E5.");

                        task.RuleFor(x => x.Icon)
                            .NotEmpty()
                            .WithMessage("أيقونة المهمة مطلوبة.");
                    });
            });
        }
    }
}
