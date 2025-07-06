using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public new DateTime CreatedAt { get; set; }
        public new string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
