using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public interface IEntity
    {
        Guid Id { get; }
        DateTime CreatedAt { get; }
        Guid? CreatedBy { get; }
        DateTime? LastModifiedAt { get; }
        Guid? LastModifiedBy { get; }
        DateTime? DeletedAt { get; }
        Guid? DeletedBy { get; }
        bool IsDeleted { get; }
    }
}