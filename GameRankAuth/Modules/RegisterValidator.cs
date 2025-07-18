using FluentValidation;
using GameRankAuth.Models;
namespace GameRankAuth.Modules;
using System.Text.RegularExpressions;

public class RegisterValidator: AbstractValidator<RegisterRequest>
{
    
    public RegisterValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().Length(3 , 15).Matches(@".*[a-zA-Z].*").WithMessage("Имя пользователя должно быть не короче 3-х и не длиннее 15 символов и содержать хотя-бы 1 букву");
        RuleFor(c =>c.Password).NotEmpty().Length(6, 40).Matches(@".*[a-zA-Z].*").WithMessage("Пароль должен быть не короче 6 и не длинее 40 символов и Содержать хотя-бы одну букву");
    }
}