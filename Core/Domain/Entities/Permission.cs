using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل دسترسی‌ها (Permissions)
    /// این کلاس نماینده جدول Permissions در دیتابیس است و تعریف کننده دسترسی‌های سیستم است
    /// </summary>
    public class Permission : AggregateRoot // Changed from BaseEntity, IAuditable to AggregateRoot
    {
        // Id is inherited from BaseEntity (via AggregateRoot)
        // Auditing fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy) are inherited.

        /// <summary>
        /// نام یکتا و کوتاه دسترسی (مثلاً "CanEdit", "CanDelete")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; private set; }
        public string NormalizedName => Name?.ToUpperInvariant();
        public string Group { get; private set; } = "Default";
        public string Type { get; private set; } = "General";
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// توضیح کامل‌تر یا اختیاری درباره عملکرد این دسترسی
        /// </summary>
        [MaxLength(200)]
        public string Description { get; private set; }

        /// <summary>
        /// نام سامانه‌ای که این دسترسی به آن تعلق دارد
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SystemName { get; private set; }

        // Removed manual Auditing properties as they are inherited.
        // public DateTime CreatedAt { get; set; }
        // public string CreatedBy { get; set; }
        // public DateTime? LastModifiedAt { get; set; }
        // public string LastModifiedBy { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedAt { get; set; }
        // public string DeletedBy { get; set; }

        /// <summary>
        /// ارتباط‌های این دسترسی با نقش‌ها
        /// </summary>
        private readonly List<RolePermission> _rolePermissions = new();
        public virtual IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        /// <summary>
        /// نقش‌های مرتبط با این دسترسی
        /// </summary>
        public virtual IReadOnlyCollection<Role> Roles => _rolePermissions.Select(rp => rp.Role).ToList().AsReadOnly();

        /// <summary>
        /// ارتباط‌های این دسترسی با کاربران
        /// </summary>
        public virtual ICollection<UserPermission> UserPermissions { get; set; }

        /// <summary>
        /// سازنده پیش‌فرض برای EF Core
        /// </summary>
        protected Permission() { }

        /// <summary>
        /// ایجاد دسترسی جدید
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="systemName">نام سامانه</param>
        /// <param name="description">توضیح اختیاری</param>
        /// <returns>نمونه جدید از Permission</returns>
        public Permission(string name, string systemName, string description = null, string group = "Default", string type = "General")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(name));
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("نام سامانه نمی‌تواند خالی باشد", nameof(systemName));

            // Id and CreatedAt are set by BaseEntity constructor
            Name = name.Trim();
            SystemName = systemName.Trim();
            Description = description?.Trim();
            Group = group;
            Type = type;
            // MarkAsCreated(null); // Optional: if you need to set CreatedBy immediately and not rely on DbContext
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات دسترسی
        /// </summary>
        /// <param name="name">نام جدید</param>
        /// <param name="systemName">نام سامانه جدید</param>
        /// <param name="description">توضیح جدید</param>
        public void UpdateDetails(string name, string systemName, string description = null, string group = null, string type = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(name));
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("نام سامانه نمی‌تواند خالی باشد", nameof(systemName));

            Name = name.Trim();
            SystemName = systemName.Trim();
            Description = description?.Trim();
            if (group != null) Group = group;
            if (type != null) Type = type;
            UpdateAuditable(null); // Assuming modifiedByUserId will be set by the caller or DbContext
        }

        public void Activate(string? activatedByUserId = null)
        {
            IsActive = true;
            UpdateAuditable(activatedByUserId);
        }

        public void Deactivate(string? deactivatedByUserId = null)
        {
            IsActive = false;
            UpdateAuditable(deactivatedByUserId);
        }

        // MarkAsDeleted is inherited from BaseEntity, no need to redefine unless specific logic is needed.
        // public void MarkAsDeleted(string deletedBy)
        // {
        //     IsDeleted = true;
        //     DeletedAt = DateTime.UtcNow;
        //     DeletedBy = deletedBy;
        // }

        /// <summary>
        /// حذف دسترسی (soft delete)
        /// </summary>
        public void DeletePermission(string? deletedByUserId = null)
        {
            DeleteAuditable(deletedByUserId);
        }

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        /// <param name="description">توضیح اختیاری برای ارتباط</param>
        public void AddToRole(Role role, string description = null)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (!_rolePermissions.Any(rp => rp.RoleId == role.Id))
            {
                var rolePermission = RolePermission.Create(role.Id, Id, description);
                _rolePermissions.Add(rolePermission);
                UpdateAuditable(null); // Assuming modifiedByUserId will be set by the caller or DbContext
            }
        }

        /// <summary>
        /// حذف دسترسی از نقش
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        public void RemoveFromRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.RoleId == role.Id);
            if (rolePermission != null)
            {
                _rolePermissions.Remove(rolePermission);
                UpdateAuditable(null); // Assuming modifiedByUserId will be set by the caller or DbContext
            }
        }

        /// <summary>
        /// بررسی وجود دسترسی در نقش
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        /// <returns>آیا دسترسی در نقش وجود دارد؟</returns>
        public bool IsInRole(Role role)
        {
            return role != null && _rolePermissions.Any(rp => rp.RoleId == role.Id);
        }
    }
}