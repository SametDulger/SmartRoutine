using AutoMapper;
using MediatR;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.ValueObjects;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Application.Common.Interfaces;
using BCrypt.Net;

namespace SmartRoutine.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDomainEventService _domainEventService;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDomainEventService domainEventService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _domainEventService = domainEventService;
    }

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        // Check if user already exists
        var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Email == email.Value)).FirstOrDefault();
        if (existingUser != null)
        {
            throw new ValidationException("Bu email adresi zaten kullanımda.");
        }
        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        // Create user
        var user = new User(email, passwordHash, request.Role ?? UserRole.User);
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Dispatch domain events
        await _domainEventService.PublishAsync(user.DomainEvents, cancellationToken);
        user.ClearDomainEvents();
        return _mapper.Map<UserDto>(user);
    }
} 