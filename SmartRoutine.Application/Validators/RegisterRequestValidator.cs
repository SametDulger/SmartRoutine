using FluentValidation;
using SmartRoutine.Application.DTOs.Auth;

namespace SmartRoutine.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi gereklidir.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(255).WithMessage("Email adresi 255 karakterden uzun olamaz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre gereklidir.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Şifre 100 karakterden uzun olamaz.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("Şifre en az bir küçük harf, bir büyük harf ve bir rakam içermelidir.");
    }
} 