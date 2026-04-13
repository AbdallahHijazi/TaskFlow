using FluentValidation;
using TaskFlow.Application.Features.Auth.Commands;

namespace TaskFlow.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("بيانات تسجيل الدخول مطلوبة");
        RuleFor(x => x.Dto.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");
        RuleFor(x => x.Dto.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة");
    }
}
