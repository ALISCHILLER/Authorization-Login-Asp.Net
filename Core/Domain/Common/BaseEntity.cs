using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public abstract class BaseEntity : IEntity, IDeletable // Added IDeletable
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; } // Changed to string? based on ICurrentUserService
        public DateTime? UpdatedAt { get; protected set; } // Renamed from LastModifiedAt for consistency
        public string? UpdatedBy { get; protected set; } // Renamed from LastModifiedBy and changed to string?
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        // It's generally preferred to have Application/Infrastructure layer handle setting these via DbContext or services
        // rather than methods directly on the entity, to keep domain entities cleaner.
        // However, retaining similar logic for now, but they might be removed if DbContext handles all auditing.

        public virtual void MarkAsCreated(string? createdByUserId)
        {
            CreatedAt = DateTime.UtcNow; // Should be set on construction or by DbContext
            CreatedBy = createdByUserId;
            UpdatedAt = null;
            UpdatedBy = null;
        }

        public virtual void MarkAsUpdated(string? updatedByUserId)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedByUserId;
        }

        public virtual void MarkAsDeleted(string? deletedByUserId)
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                DeletedAt = DateTime.UtcNow;
                DeletedBy = deletedByUserId;
                UpdatedAt = DateTime.UtcNow; // Also mark as updated when soft deleting
                UpdatedBy = deletedByUserId;
            }
        }

        public virtual void MarkAsRestored(string? restoredByUserId) // Renamed from Restore
        {
            if (IsDeleted)
            {
                IsDeleted = false;
                DeletedAt = null;
                DeletedBy = null;
                UpdatedAt = DateTime.UtcNow;
                UpdatedBy = restoredByUserId;
            }
        }
    }
}