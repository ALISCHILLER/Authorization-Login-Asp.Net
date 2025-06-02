using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Roles
{
    /// <summary>
    /// اطلاعات نقش
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// شناسه نقش
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// نام نقش
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// توضیحات نقش
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا نقش سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// دسترسی‌های نقش
        /// </summary>
        public List<PermissionDto> Permissions { get; set; }

        /// <summary>
        /// تعداد کاربران دارای این نقش
        /// </summary>
        public int UserCount { get; set; }
    }

    /// <summary>
    /// درخواست ایجاد نقش جدید
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// نام نقش
        /// </summary>
        [Required(ErrorMessage = "نام نقش الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام نقش باید بین 3 تا 50 کاراکتر باشد")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات نقش
        /// </summary>
        [StringLength(200, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// آیا نقش سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// دسترسی‌های نقش
        /// </summary>
        public List<string> Permissions { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی نقش
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// نام نقش
        /// </summary>
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام نقش باید بین 3 تا 50 کاراکتر باشد")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات نقش
        /// </summary>
        [StringLength(200, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// دسترسی‌های نقش
        /// </summary>
        public List<string> Permissions { get; set; }
    }

    /// <summary>
    /// اطلاعات دسترسی
    /// </summary>
    public class PermissionDto
    {
        /// <summary>
        /// شناسه دسترسی
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// نام دسترسی
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// توضیحات دسترسی
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// گروه دسترسی
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// آیا دسترسی سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }
    }

    /// <summary>
    /// درخواست ایجاد دسترسی جدید
    /// </summary>
    public class CreatePermissionRequest
    {
        /// <summary>
        /// نام دسترسی
        /// </summary>
        [Required(ErrorMessage = "نام دسترسی الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام دسترسی باید بین 3 تا 50 کاراکتر باشد")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات دسترسی
        /// </summary>
        [StringLength(200, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// گروه دسترسی
        /// </summary>
        [Required(ErrorMessage = "گروه دسترسی الزامی است")]
        [StringLength(50, ErrorMessage = "نام گروه نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string Group { get; set; }

        /// <summary>
        /// آیا دسترسی سیستمی است
        /// </summary>
        public bool IsSystem { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی دسترسی
    /// </summary>
    public class UpdatePermissionRequest
    {
        /// <summary>
        /// نام دسترسی
        /// </summary>
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام دسترسی باید بین 3 تا 50 کاراکتر باشد")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات دسترسی
        /// </summary>
        [StringLength(200, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// گروه دسترسی
        /// </summary>
        [StringLength(50, ErrorMessage = "نام گروه نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string Group { get; set; }
    }

    /// <summary>
    /// درخواست اختصاص نقش به کاربر
    /// </summary>
    public class AssignRoleRequestDto : BaseRequestDto
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
    }

    /// <summary>
    /// درخواست حذف نقش از کاربر
    /// </summary>
    public class RemoveRoleRequestDto : BaseRequestDto
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
    }
} 