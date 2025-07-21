using FluentValidation;
using GameRankAuth.Models;

namespace GameRankAuth.Modules;

public class ChangePasswordValidator:AbstractValidator<UserData.ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().Length(6, 40).Matches(@".*[a-zA-Z].*").WithMessage("Пароль должен быть не короче 6 и не длинее 40 символов и Содержать хотя-бы одну букву");
    }
}