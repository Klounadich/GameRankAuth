using FluentValidation;

namespace GameRankAuth.Modules;

public class DescriptionValidator:AbstractValidator<string>
{
    public DescriptionValidator()
    {
        RuleFor(x => x).MaximumLength(100).WithMessage("Описание не может быть длинее 100 символов");
    }
}