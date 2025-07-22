using AutoMapper;
using MediatR;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.ValueObjects;
using BCrypt.Net;

namespace SmartRoutine.Application.Commands.Auth;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        // Update display name if provided
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            user.UpdateDisplayName(request.DisplayName);
        }

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var newEmail = Email.Create(request.Email);
            user.UpdateEmail(newEmail);
        }

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                throw new ValidationException("Mevcut şifre gereklidir.");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new ValidationException("Mevcut şifre yanlış.");
            }

            // Hash and update new password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatePassword(newPasswordHash);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return _mapper.Map<UserDto>(user);
    }
} 