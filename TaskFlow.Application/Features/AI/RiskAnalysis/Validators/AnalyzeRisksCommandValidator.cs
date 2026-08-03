using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Features.AI.RiskAnalysis.Commands;

namespace TaskFlow.Application.Features.AI.RiskAnalysis.Validators
{
    public sealed class AnalyzeRisksCommandValidator
        : AbstractValidator<AnalyzeRisksCommand>
    {
        public AnalyzeRisksCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("بيانات طلب تحليل المخاطر مطلوبة.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.InitiativeId)
                    .NotEmpty()
                    .WithMessage("معرّف المبادرة مطلوب.");
            });
        }
    }
}
