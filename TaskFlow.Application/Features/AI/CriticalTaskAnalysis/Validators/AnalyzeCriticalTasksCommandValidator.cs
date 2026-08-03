using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Commands;

namespace TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Validators
{
    public sealed class AnalyzeCriticalTasksCommandValidator
        : AbstractValidator<AnalyzeCriticalTasksCommand>
    {
        public AnalyzeCriticalTasksCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("بيانات طلب تحليل المهام الحرجة مطلوبة.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.InitiativeId)
                    .NotEmpty()
                    .WithMessage("معرّف المبادرة مطلوب.");
            });
        }
    }
}
