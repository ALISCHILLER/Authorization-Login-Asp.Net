using AutoMapper;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Infrastructure.Mappings
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
            // نگاشت از موجودیت User به DTO (UserDto) با تبدیل فیلدهای خاص (مثلاً تبدیل Email به مقدار رشته‌ای و تبدیل Role به رشته)
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // نگاشت از درخواست ثبت‌نام (RegisterRequest) به موجودیت User (با تبدیل فیلدهای خاص و نادیده گرفتن فیلدهای هش رمز عبور و رفرش توکن‌ها)
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Domain.ValueObjects.Email(src.Email)))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<Domain.Enums.UserRole>(src.Role)))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // رمز عبور هش می‌شود جای دیگر
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Email)) // به عنوان مثال Username = Email
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

            // نگاشت از موجودیت User به پاسخ احراز هویت (AuthResponse) با تبدیل فیلدهای خاص و نادیده گرفتن فیلدهای توکن و پرمیشن‌ها (که معمولاً جداگانه پر می‌شوند)
            CreateMap<User, AuthResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // معمولاً جداگانه پر می‌شود
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.AccessTokenExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiresAt, opt => opt.Ignore());

            // نگاشت از موجودیت LoginHistory به DTO (LoginHistoryDto)
            CreateMap<LoginHistory, LoginHistoryDto>();

            // نگاشت از درخواست ثبت ورود (LoginHistoryRequestDto) به موجودیت LoginHistory
            CreateMap<LoginHistoryRequestDto, LoginHistory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // شناسه توسط سرویس تولید می‌شود
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // شناسه کاربر توسط سرویس تنظیم می‌شود
                .ForMember(dest => dest.LoginTime, opt => opt.Ignore()) // زمان ورود توسط سرویس تنظیم می‌شود
                .ForMember(dest => dest.IsSuccessful, opt => opt.Ignore()) // وضعیت موفقیت توسط سرویس تنظیم می‌شود
                .ForMember(dest => dest.LogoutTime, opt => opt.Ignore()) // زمان خروج توسط سرویس تنظیم می‌شود
                .ForMember(dest => dest.SessionDuration, opt => opt.Ignore()) // مدت زمان حضور توسط سرویس محاسبه می‌شود
                .ForMember(dest => dest.User, opt => opt.Ignore()); // رابطه با کاربر توسط سرویس تنظیم می‌شود
        }
    }
} 