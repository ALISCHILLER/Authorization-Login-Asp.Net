using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class UserPermission : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PermissionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Permission Permission { get; set; }
    }
} 