using AutoMapper;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Authorization_Login_Asp.Net.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User → UserDto
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // RegisterRequest → User
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Domain.ValueObjects.Email(src.Email)))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<Domain.Enums.UserRole>(src.Role)))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // رمز عبور هش می‌شود جای دیگر
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Email)) // به عنوان مثال Username = Email
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

            // User → AuthResponse
            CreateMap<User, AuthResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // معمولاً جداگانه پر می‌شود
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.AccessTokenExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiresAt, opt => opt.Ignore());
        }
    }
}
