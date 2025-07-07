using AutoMapper;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
// using Authorization_Login_Asp.Net.Core.Application.DTOs; // Removed as no longer needed
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users; // For UserDto, LoginHistoryDto, CreateUserRequest
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth; // For AuthResponse

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Mappings
{
    /// <summary>
    /// پروفایل نگاشت (Mapping) برای تبدیل بین موجودیت‌ها و مدل‌های DTO با استفاده از AutoMapper؛ این کلاس نگاشت‌های مورد نیاز (مثلا از موجودیت User به UserDto، از RegisterRequest به User و از User به AuthResponse) را تعریف می‌کند.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// سازنده پروفایل نگاشت؛ در این متد نگاشت‌های مورد نیاز بین موجودیت‌ها و مدل‌های DTO تعریف می‌شوند.
        /// </summary>
        public MappingProfile()
        {
            // نگاشت از موجودیت User به DTO (UserDto)
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                .ForMember(dest => dest.PrimaryRole, opt => opt.MapFrom(src => src.Roles.Count > 0 ? src.Roles.First().Name : string.Empty));

            // نگاشت از درخواست ثبت‌نام (CreateUserRequest) به موجودیت User
            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Domain.ValueObjects.Email(src.Email)))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

            // نگاشت از موجودیت User به پاسخ احراز هویت (AuthResponse)
            CreateMap<User, AuthResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));

            // نگاشت از موجودیت LoginHistory به DTO (LoginHistoryDto)
            CreateMap<LoginHistory, LoginHistoryDto>()
                .ForMember(dest => dest.Browser, opt => opt.MapFrom(src => src.Browser))
                .ForMember(dest => dest.Device, opt => opt.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.SessionDuration, opt => opt.MapFrom(src => src.SessionDuration));

            // نگاشت از DTO (LoginHistoryDto) به موجودیت LoginHistory
            CreateMap<LoginHistoryDto, LoginHistory>()
                .ForMember(dest => dest.Browser, opt => opt.MapFrom(src => src.Browser))
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Device))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.SessionDuration, opt => opt.MapFrom(src => src.SessionDuration));
        }
    }
}