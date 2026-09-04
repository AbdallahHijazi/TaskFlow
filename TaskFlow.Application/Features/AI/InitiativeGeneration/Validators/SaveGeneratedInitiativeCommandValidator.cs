using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Validators
{
    public sealed class SaveGeneratedInitiativeCommandValidator
        : AbstractValidator<SaveGeneratedInitiativeCommand>
    {
        public SaveGeneratedInitiativeCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("بيانات المبادرة مطلوبة.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.Name)
                    .NotEmpty()
                    .WithMessage("اسم المبادرة مطلوب.")
                    .MaximumLength(200)
                    .WithMessage("اسم المبادرة يجب ألا يتجاوز 200 حرف.");

                RuleFor(x => x.Request.Description)
                    .NotEmpty()
                    .WithMessage("وصف المبادرة مطلوب.")
                    .MaximumLength(2000)
                    .WithMessage("وصف المبادرة يجب ألا يتجاوز 2000 حرف.");

                RuleFor(x => x.Request.StartDate)
                    .NotEmpty()
                    .WithMessage("تاريخ بداية المبادرة مطلوب.");

                RuleFor(x => x.Request.EndDate)
                    .NotNull()
                    .WithMessage("تاريخ نهاية المبادرة مطلوب.")
                    .GreaterThanOrEqualTo(x => x.Request.StartDate)
                    .WithMessage(
                        "تاريخ نهاية المبادرة يجب ألا يسبق تاريخ البداية.");

                RuleFor(x => x.Request.Color)
                    .NotEmpty()
                    .Matches("^#[0-9A-Fa-f]{6}$")
                    .WithMessage("لون المبادرة غير صالح.");

                RuleFor(x => x.Request.Icon)
                    .NotEmpty()
                    .WithMessage("أيقونة المبادرة مطلوبة.");

                RuleFor(x => x.Request.Tasks)
                    .NotNull()
                    .WithMessage("قائمة المهام مطلوبة.")
                    .Must(tasks => tasks is { Count: > 0 })
                    .WithMessage("يجب وجود مهمة واحدة على الأقل.");

                RuleForEach(x => x.Request.Tasks)
                    .ChildRules(task =>
                    {
                        task.RuleFor(x => x.Name)
                            .NotEmpty()
                            .WithMessage("اسم المهمة مطلوب.")
                            .MaximumLength(200)
                            .WithMessage(
                                "اسم المهمة يجب ألا يتجاوز 200 حرف.");

                        task.RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage("وصف المهمة مطلوب.")
                            .MaximumLength(2000)
                            .WithMessage(
                                "وصف المهمة يجب ألا يتجاوز 2000 حرف.");

                        task.RuleFor(x => x.StartDate)
                            .NotEmpty()
                            .WithMessage("تاريخ بداية المهمة مطلوب.");

                        task.RuleFor(x => x.EndDate)
                            .NotNull()
                            .WithMessage("تاريخ نهاية المهمة مطلوب.")
                            .GreaterThanOrEqualTo(x => x.StartDate)
                            .WithMessage(
                                "تاريخ نهاية المهمة يجب ألا يسبق تاريخ بدايتها.");

                        task.RuleFor(x => x.Color)
                            .NotEmpty()
                            .Matches("^#[0-9A-Fa-f]{6}$")
                            .WithMessage("لون المهمة غير صالح.");

                        task.RuleFor(x => x.Icon)
                            .NotEmpty()
                            .WithMessage("أيقونة المهمة مطلوبة.");
                    });

                RuleFor(x => x.Request)
                    .Must(HaveUniqueTaskNames)
                    .WithMessage("يجب ألا تتكرر أسماء المهام.");

                RuleFor(x => x.Request)
                    .Must(HaveTasksInsideInitiativeDates)
                    .WithMessage(
                        "يجب أن تقع جميع تواريخ المهام ضمن مدة المبادرة.");
            });
        }

        private static bool HaveUniqueTaskNames(
            DTOs.AI.InitiativeGeneration.SaveGeneratedInitiativeRequest request)
        {
            if (request.Tasks is null)
            {
                return false;
            }

            var validNames = request.Tasks
                .Where(task => !string.IsNullOrWhiteSpace(task.Name))
                .Select(task => task.Name.Trim())
                .ToList();

            return validNames.Count ==
                   validNames.Distinct(
                       StringComparer.OrdinalIgnoreCase).Count();
        }

        private static bool HaveTasksInsideInitiativeDates(
            DTOs.AI.InitiativeGeneration.SaveGeneratedInitiativeRequest request)
        {
            if (request.Tasks is null ||
                request.EndDate is null)
            {
                return false;
            }

            return request.Tasks.All(task =>
                task.StartDate >= request.StartDate &&
                task.EndDate is not null &&
                task.EndDate <= request.EndDate);
        }
    }
}
