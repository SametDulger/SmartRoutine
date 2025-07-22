using AutoMapper;
using MediatR;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Queries.Auth;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {request.UserId}");
        }
        return _mapper.Map<UserDto>(user);
    }
} 