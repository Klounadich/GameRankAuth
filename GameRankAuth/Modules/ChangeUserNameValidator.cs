using FluentValidation;

namespace GameRankAuth.Modules;

public class ChangeUserNameValidator:AbstractValidator<string>
{
   public ChangeUserNameValidator()
    {
        RuleFor(x =>x).NotEmpty().Length(3 , 15).Matches(@".*[a-zA-Z].*").WithMessage("Имя пользователя должно быть не короче 3-х и не длиннее 15 символов и содержать хотя-бы 1 букву");
    }
}