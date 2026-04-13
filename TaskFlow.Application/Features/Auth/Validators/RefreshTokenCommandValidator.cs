using FluentValidation;
using TaskFlow.Application.Features.Auth.Commands;

namespace TaskFlow.Application.Features.Auth.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("رمز الوصول مطلوب");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("رمز التحديث مطلوب");
    }
}
