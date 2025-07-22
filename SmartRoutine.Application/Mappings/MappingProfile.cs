using AutoMapper;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Application.DTOs.Stats;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Value Object mappings
        CreateMap<Email, string>().ConvertUsing(email => email.Value);
        CreateMap<string, Email>().ConvertUsing(str => Email.Create(str));
        
        // Time mappings
        CreateMap<string, TimeOnly>().ConvertUsing(str => TimeOnly.Parse(str));
        CreateMap<TimeOnly, string>().ConvertUsing(time => time.ToString("HH:mm"));

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => Email.Create(src.Email)))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Routines, opt => opt.Ignore());

        // Routine mappings
        CreateMap<Routine, RoutineDto>()
            .ForMember(dest => dest.IsCompletedToday, opt => opt.Ignore());
        CreateMap<RoutineCreateDto, Routine>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.RoutineLogs, opt => opt.Ignore());
        CreateMap<RoutineUpdateDto, Routine>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.RoutineLogs, opt => opt.Ignore());

        // RoutineLog mappings
        CreateMap<RoutineLog, RoutineDto>();
    }
} 