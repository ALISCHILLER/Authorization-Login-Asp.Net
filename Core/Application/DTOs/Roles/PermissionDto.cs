using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Roles
{
    /// <summary>
    /// کلاس درخواست ایجاد دسترسی جدید
    /// </summary>
    public class CreatePermissionRequestDto : BaseRequestDto
    {
        /// <summary>
        /// نام دسترسی
        /// </summary>
        [Required(ErrorMessage = "نام دسترسی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام دسترسی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "نام دسترسی فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد")]
        public string Name { get; set; }

        /// <summary>
        /// عنوان نمایشی دسترسی
        /// </summary>
        [Required(ErrorMessage = "عنوان نمایشی دسترسی الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نمایشی دسترسی نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات دسترسی
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات دسترسی نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// گروه دسترسی
        /// </summary>
        [Required(ErrorMessage = "گروه دسترسی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام گروه دسترسی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string Group { get; set; }

        /// <summary>
        /// اولویت نمایش دسترسی
        /// </summary>
        [Range(1, 100, ErrorMessage = "اولویت نمایش دسترسی باید بین 1 تا 100 باشد")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// آیا دسترسی سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// دسترسی‌های وابسته
        /// </summary>
        public List<string> DependentPermissions { get; set; }
    }

    /// <summary>
    /// کلاس درخواست به‌روزرسانی دسترسی
    /// </summary>
    public class UpdatePermissionRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه دسترسی
        /// </summary>
        [Required(ErrorMessage = "شناسه دسترسی الزامی است")]
        public Guid Id { get; set; }

        /// <summary>
        /// عنوان نمایشی دسترسی
        /// </summary>
        [Required(ErrorMessage = "عنوان نمایشی دسترسی الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان نمایشی دسترسی نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات دسترسی
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات دسترسی نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// گروه دسترسی
        /// </summary>
        [Required(ErrorMessage = "گروه دسترسی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام گروه دسترسی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string Group { get; set; }

        /// <summary>
        /// اولویت نمایش دسترسی
        /// </summary>
        [Range(1, 100, ErrorMessage = "اولویت نمایش دسترسی باید بین 1 تا 100 باشد")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// دسترسی‌های وابسته
        /// </summary>
        public List<string> DependentPermissions { get; set; }
    }

    /// <summary>
    /// کلاس درخواست تخصیص دسترسی به نقش
    /// </summary>
    public class AssignPermissionToRoleRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required(ErrorMessage = "شناسه نقش الزامی است")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// لیست شناسه‌های دسترسی
        /// </summary>
        [Required(ErrorMessage = "لیست دسترسی‌ها الزامی است")]
        public List<Guid> PermissionIds { get; set; }

        /// <summary>
        /// توضیحات تخصیص
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات تخصیص نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string AssignmentNote { get; set; }
    }

    /// <summary>
    /// کلاس درخواست حذف دسترسی از نقش
    /// </summary>
    public class RemovePermissionFromRoleRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        [Required(ErrorMessage = "شناسه نقش الزامی است")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// لیست شناسه‌های دسترسی
        /// </summary>
        [Required(ErrorMessage = "لیست دسترسی‌ها الزامی است")]
        public List<Guid> PermissionIds { get; set; }

        /// <summary>
        /// دلیل حذف دسترسی
        /// </summary>
        [Required(ErrorMessage = "دلیل حذف دسترسی الزامی است")]
        [MaxLength(500, ErrorMessage = "دلیل حذف دسترسی نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string RemovalReason { get; set; }
    }

    /// <summary>
    /// کلاس پاسخ مدیریت دسترسی
    /// </summary>
    public class PermissionResponseDto : BaseResponseDto
    {
        /// <summary>
        /// شناسه دسترسی
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// نام دسترسی
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// عنوان نمایشی دسترسی
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات دسترسی
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// گروه دسترسی
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// اولویت نمایش دسترسی
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// آیا دسترسی سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// دسترسی‌های وابسته
        /// </summary>
        public List<string> DependentPermissions { get; set; }

        /// <summary>
        /// تاریخ ایجاد دسترسی
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی دسترسی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تعداد نقش‌های دارای این دسترسی
        /// </summary>
        public int RoleCount { get; set; }

        /// <summary>
        /// لیست نقش‌های دارای این دسترسی
        /// </summary>
        public List<PermissionRoleInfo> Roles { get; set; }
    }

    /// <summary>
    /// کلاس اطلاعات نقش دارای دسترسی
    /// </summary>
    public class PermissionRoleInfo
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// نام نقش
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// عنوان نمایشی نقش
        /// </summary>
        public string RoleDisplayName { get; set; }

        /// <summary>
        /// نوع نقش
        /// </summary>
        public string RoleType { get; set; }

        /// <summary>
        /// تاریخ تخصیص دسترسی
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// توضیحات تخصیص
        /// </summary>
        public string AssignmentNote { get; set; }
    }

    /// <summary>
    /// کلاس گروه‌بندی دسترسی‌ها
    /// </summary>
    public class PermissionGroupDto
    {
        /// <summary>
        /// نام گروه
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// عنوان نمایشی گروه
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات گروه
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// اولویت نمایش گروه
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// لیست دسترسی‌های گروه
        /// </summary>
        public List<PermissionResponseDto> Permissions { get; set; }
    }
} 