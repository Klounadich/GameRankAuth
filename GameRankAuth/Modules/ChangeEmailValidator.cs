using FluentValidation;

namespace GameRankAuth.Modules;

public class ChangeEmailValidator:AbstractValidator<string>
{
    public ChangeEmailValidator()
    {
        RuleFor(x =>x).NotEmpty().EmailAddress().WithMessage("Передана строка не содержащая email адрес");
    }
}