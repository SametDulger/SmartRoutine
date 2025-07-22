using FluentValidation;
using SmartRoutine.Application.Commands.Routines;

namespace SmartRoutine.Application.Validators;

public class DeleteRoutineCommandValidator : AbstractValidator<DeleteRoutineCommand>
{
    public DeleteRoutineCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID gereklidir.");
            
        RuleFor(x => x.RoutineId)
            .NotEmpty()
            .WithMessage("Rutin ID gereklidir.");
    }
} 