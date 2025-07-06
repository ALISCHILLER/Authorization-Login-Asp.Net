using AutoMapper;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.Common.Models;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Roles;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Mappings
{
    /// <summary>
    /// پروفایل مپینگ AutoMapper برای تبدیل بین موجودیت‌ها و DTOها
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// سازنده پروفایل مپینگ
        /// </summary>
        public MappingProfile()
        {
            // مپینگ کاربر
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name ?? string.Empty,
                    Description = ur.Role.Description ?? string.Empty,
                    IsActive = ur.Role.IsActive,
                    Permissions = ur.Role.Permissions != null ? ur.Role.Permissions.Permissions.ToList() : new List<string>()
                }).ToList()))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                    src.UserRoles.SelectMany(ur => ur.Role.Permissions != null ? ur.Role.Permissions.Permissions : new List<string>()).Distinct().ToList()));

            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.UserRoles.Select(ur => ur.Role.Name ?? string.Empty).ToList()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username ?? string.Empty));

            CreateMap<User, UserSecuritySettingsDto>()
                .ForMember(dest => dest.HasTwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
                .ForMember(dest => dest.LastPasswordChangeDate, opt => opt.MapFrom(src => src.LastPasswordChange))
                .ForMember(dest => dest.RequiresPasswordChange, opt => opt.MapFrom(src =>
                    (src.LastPasswordChange ?? DateTime.Now).AddDays(90) <= DateTime.UtcNow));

            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.ToLower()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Username.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));

            CreateMap<UpdateUserRequest, User>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.ToLower()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Username.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // مپینگ نقش
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                    src.Permissions != null ? src.Permissions.Permissions.ToList() : new List<string>()));

            CreateMap<CreateRoleRequest, Role>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description?.Trim()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateRoleRequest, Role>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description?.Trim()))
                .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // مپینگ دسترسی
            CreateMap<Permission, PermissionDto>();

            CreateMap<CreatePermissionRequest, Permission>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePermissionRequest, Permission>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // مپینگ تاریخچه ورود
            CreateMap<LoginHistory, LoginHistoryDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            // مپینگ دستگاه کاربر
            CreateMap<UserDevice, UserDeviceDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            // مپینگ توکن رفرش
            CreateMap<RefreshToken, RefreshTokenDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            // مپینگ کدهای بازیابی دو مرحله‌ای
            CreateMap<TwoFactorRecoveryCode, TwoFactorRecoveryCodeDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));
        }
    }
}