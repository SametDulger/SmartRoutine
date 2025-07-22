using FluentValidation;
using SmartRoutine.Application.DTOs.Auth;

namespace SmartRoutine.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Görünen ad 100 karakterden uzun olamaz.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.NewPassword)
            .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre gereklidir.")
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));
    }
} 