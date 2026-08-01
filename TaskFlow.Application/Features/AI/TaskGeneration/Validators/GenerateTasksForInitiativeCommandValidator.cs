using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Features.AI.TaskGeneration.Commands;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Validators
{
    public sealed class GenerateTasksForInitiativeCommandValidator
        : AbstractValidator<GenerateTasksForInitiativeCommand>
    {
        public GenerateTasksForInitiativeCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("بيانات طلب توليد المهام مطلوبة.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.InitiativeId)
                    .NotEmpty()
                    .WithMessage("معرّف المبادرة مطلوب.");

                RuleFor(x => x.Request.Prompt)
                    .NotEmpty()
                    .WithMessage("وصف المهام المطلوبة مطلوب.")
                    .MinimumLength(5)
                    .WithMessage("وصف المهام قصير جدًا.")
                    .MaximumLength(1000)
                    .WithMessage("وصف المهام يجب ألا يتجاوز 1000 حرف.");
            });
        }
    }
}
