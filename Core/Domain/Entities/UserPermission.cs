using System;

using Authorization_Login_Asp.Net.Core.Domain.Common; // Added for BaseEntity

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class UserPermission : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy are inherited.

        public Guid UserId { get; set; }
        public Guid PermissionId { get; set; }

        // Removed manual Id and Auditing properties.

        public virtual User User { get; set; }
        public virtual Permission Permission { get; set; }

        // Default constructor for EF Core, calls base constructor
        public UserPermission() : base() { }

        // Optional: A constructor to set UserId and PermissionId if needed during creation
        public UserPermission(Guid userId, Guid permissionId) : this()
        {
            UserId = userId;
            PermissionId = permissionId;
            // CreatedBy would be set by the service/context if needed via MarkAsCreated
        }
    }
} 