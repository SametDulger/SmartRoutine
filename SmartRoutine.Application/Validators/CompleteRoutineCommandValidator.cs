using FluentValidation;
using SmartRoutine.Application.Commands.Routines;

namespace SmartRoutine.Application.Validators;

public class CompleteRoutineCommandValidator : AbstractValidator<CompleteRoutineCommand>
{
    public CompleteRoutineCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID gereklidir.");
            
        RuleFor(x => x.RoutineId)
            .NotEmpty()
            .WithMessage("Rutin ID gereklidir.");
            
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notlar 500 karakterden uzun olamaz.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
} 