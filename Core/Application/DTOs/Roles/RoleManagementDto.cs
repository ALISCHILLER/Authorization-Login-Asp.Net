using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Roles
{
    /// <summary>
    /// کلاس درخواست ایجاد نقش جدید
    /// </summary>
    public class CreateRoleRequestDto : BaseRequestDto
    {
        /// <summary>
        /// نام نقش
        /// </summary>
        [Required(ErrorMessage = "نام نقش الزامی است")]
        [MaxLength(50, ErrorMessage = "نام نقش نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "نام نقش فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد")]
        public string Name { get; set; }

        /// <summary>
        /// عنوان نمایشی نقش
        /// </summary>
        [Required(ErrorMessage = "عنوان نمایشی نقش الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نمایشی نقش نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات نقش
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نقش نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// نوع نقش
        /// </summary>
        [Required(ErrorMessage = "نوع نقش الزامی است")]
        public RoleType RoleType { get; set; }

        /// <summary>
        /// دسترسی‌های نقش
        /// </summary>
        public List<string> Permissions { get; set; }

        /// <summary>
        /// آیا نقش پیش‌فرض است
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// آیا نقش سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// اولویت نمایش نقش
        /// </summary>
        [Range(1, 100, ErrorMessage = "اولویت نمایش نقش باید بین 1 تا 100 باشد")]
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// کلاس درخواست به‌روزرسانی نقش
    /// </summary>
    public class UpdateRoleRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required(ErrorMessage = "شناسه نقش الزامی است")]
        public Guid Id { get; set; }

        /// <summary>
        /// عنوان نمایشی نقش
        /// </summary>
        [Required(ErrorMessage = "عنوان نمایشی نقش الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نمایشی نقش نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات نقش
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نقش نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// دسترسی‌های نقش
        /// </summary>
        public List<string> Permissions { get; set; }

        /// <summary>
        /// آیا نقش پیش‌فرض است
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// اولویت نمایش نقش
        /// </summary>
        [Range(1, 100, ErrorMessage = "اولویت نمایش نقش باید بین 1 تا 100 باشد")]
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// کلاس درخواست تخصیص نقش به کاربر
    /// </summary>
    public class AssignRoleToUserRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required(ErrorMessage = "شناسه نقش الزامی است")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// تاریخ انقضای نقش
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// توضیحات تخصیص
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات تخصیص نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string AssignmentNote { get; set; }
    }

    /// <summary>
    /// کلاس درخواست حذف نقش از کاربر
    /// </summary>
    public class RemoveRoleFromUserRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required(ErrorMessage = "شناسه نقش الزامی است")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// دلیل حذف نقش
        /// </summary>
        [Required(ErrorMessage = "دلیل حذف نقش الزامی است")]
        [MaxLength(500, ErrorMessage = "دلیل حذف نقش نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string RemovalReason { get; set; }
    }

    /// <summary>
    /// کلاس پاسخ مدیریت نقش
    /// </summary>
    public class RoleManagementResponseDto : BaseResponseDto
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// نام نقش
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// عنوان نمایشی نقش
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات نقش
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// نوع نقش
        /// </summary>
        public RoleType RoleType { get; set; }

        /// <summary>
        /// دسترسی‌های نقش
        /// </summary>
        public List<string> Permissions { get; set; }

        /// <summary>
        /// آیا نقش پیش‌فرض است
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// آیا نقش سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// اولویت نمایش نقش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// تاریخ ایجاد نقش
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی نقش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تعداد کاربران دارای این نقش
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// لیست کاربران دارای این نقش
        /// </summary>
        public List<RoleUserInfo> Users { get; set; }
    }

    /// <summary>
    /// کلاس اطلاعات کاربر دارای نقش
    /// </summary>
    public class RoleUserInfo
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// تاریخ تخصیص نقش
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// تاریخ انقضای نقش
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// آیا نقش منقضی شده است
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// تعداد روزهای باقی‌مانده تا انقضا
        /// </summary>
        public int? DaysUntilExpiration { get; set; }

        /// <summary>
        /// توضیحات تخصیص
        /// </summary>
        public string AssignmentNote { get; set; }
    }
} 