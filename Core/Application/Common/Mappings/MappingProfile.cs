using AutoMapper;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.Common.Models;

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
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => 
                    src.UserRoles.SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))));

            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => 
                    src.UserRoles.Select(ur => new RoleDto 
                    { 
                        Id = ur.Role.Id, 
                        Name = ur.Role.Name, 
                        Description = ur.Role.Description 
                    })));

            CreateMap<User, UserSecuritySettingsDto>()
                .ForMember(dest => dest.HasTwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
                .ForMember(dest => dest.LastPasswordChangeDate, opt => opt.MapFrom(src => src.LastPasswordChangeDate))
                .ForMember(dest => dest.RequiresPasswordChange, opt => opt.MapFrom(src => 
                    src.LastPasswordChangeDate.AddDays(90) <= DateTime.UtcNow));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.ToLower()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.ToLower()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // مپینگ نقش
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => 
                    src.RolePermissions.Select(rp => rp.Permission)));

            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateRoleDto, Role>()
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // مپینگ دسترسی
            CreateMap<Permission, PermissionDto>();

            CreateMap<CreatePermissionDto, Permission>()
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePermissionDto, Permission>()
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

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