using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Validators
{
    public sealed class GenerateInitiativeCommandValidator
        : AbstractValidator<GenerateInitiativeCommand>
    {
        public GenerateInitiativeCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("بيانات طلب إنشاء المبادرة مطلوبة.");

            RuleFor(x => x.Request.Prompt)
                .NotEmpty()
                .WithMessage("وصف المبادرة مطلوب.")
                .MinimumLength(10)
                .WithMessage("وصف المبادرة قصير جدًا.")
                .MaximumLength(2000)
                .WithMessage("وصف المبادرة يجب ألا يتجاوز 2000 حرف.")
                .Must(ContainsInitiativeRequest)
                .WithMessage(
                    "يرجى كتابة وصف واضح للمبادرة المطلوبة، مثل: أنشئ مبادرة لنظام إدارة عيادة.");

            RuleFor(x => x.Request.StatusId)
                .NotEmpty()
                .WithMessage("حالة المبادرة مطلوبة.");

            RuleFor(x => x.Request.AssignedToId)
                .NotEmpty()
                .WithMessage("المستخدم المسؤول عن المبادرة مطلوب.");
        }
        private static bool ContainsInitiativeRequest(string? prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return false;
            }

            var normalized = NormalizeText(prompt);

            var greetingOnlyWords = new HashSet<string>
            {
                "السلام",
                "عليكم",
                "ورحمة",
                "الله",
                "وبركاته",
                "مرحبا",
                "مرحباً",
                "اهلا",
                "أهلا",
                "صباح",
                "الخير",
                "مساء",
                "النور",
                "كيفك",
                "كيف",
                "الحال"
            };

            var meaningfulWords = normalized
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Where(word => !greetingOnlyWords.Contains(word))
                .ToList();

            if (meaningfulWords.Count < 2)
            {
                return false;
            }

            var initiativeKeywords = new[]
            {
                "مبادرة",
                "مشروع",
                "نظام",
                "تطبيق",
                "منصة",
                "برنامج",
                "خدمة",
                "إنشاء",
                "انشاء",
                "تطوير",
                "إدارة",
                "ادارة",
                "أتمتة",
                "اتمتة"
            };

            return initiativeKeywords.Any(
                keyword => normalized.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeText(string text)
        {
            return text
                .Replace("،", " ")
                .Replace(".", " ")
                .Replace("!", " ")
                .Replace("؟", " ")
                .Replace("?", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();
        }
    }
}
