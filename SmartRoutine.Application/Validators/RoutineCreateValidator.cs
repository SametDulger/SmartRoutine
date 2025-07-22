using FluentValidation;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Validators;

public class RoutineCreateValidator : AbstractValidator<RoutineCreateDto>
{
    public RoutineCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Rutin başlığı gereklidir.")
            .MaximumLength(200).WithMessage("Rutin başlığı 200 karakterden uzun olamaz.")
            .Matches("^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ\\s.,!?-]+$")
            .WithMessage("Rutin başlığı özel karakter içeremez.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakterden uzun olamaz.")
            .Matches("^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ\\s.,!?-]*$")
            .WithMessage("Açıklama özel karakter içeremez.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.TimeOfDay)
            .NotEmpty().WithMessage("Rutin saati gereklidir.");

        RuleFor(x => x.RepeatType)
            .NotEmpty().WithMessage("Tekrar türü gereklidir.")
            .Must(BeValidRepeatType).WithMessage("Geçersiz tekrar türü.");

        RuleFor(x => x.RepeatDays)
            .Must(days => days == null || (days.Count >= 1 && days.Count <= 7))
            .WithMessage("Haftalık tekrar günleri 1 ile 7 gün arasında olmalıdır.")
            .When(x => x.RepeatType == "Weekly");

        RuleFor(x => x)
            .CustomAsync(async (dto, context, cancellation) => {
                // Burada UserId ve IUnitOfWork/IRepository erişimi gerekecek, örnek olarak bırakıldı
                // var userRoutineCount = await ...
                // if (userRoutineCount > 50) context.AddFailure("Bir kullanıcı haftada en fazla 50 rutin oluşturabilir.");
            });
    }

    private static bool BeValidRepeatType(string repeatType)
    {
        return repeatType switch
        {
            "Daily" => true,
            "Weekly" => true,
            "CustomDays" => true,
            "IntervalBased" => true,
            _ => false
        };
    }
} 