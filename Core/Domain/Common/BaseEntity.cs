using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public abstract class BaseEntity : IEntity
    {
        private Guid _id;
        private DateTime _createdAt;
        private Guid? _createdBy;
        private DateTime? _lastModifiedAt;
        private Guid? _lastModifiedBy;
        private DateTime? _deletedAt;
        private Guid? _deletedBy;
        private bool _isDeleted;

        protected BaseEntity()
        {
            _id = Guid.NewGuid();
            _createdAt = DateTime.UtcNow;
        }

        public virtual Guid Id { get; protected set; }
        public virtual DateTime CreatedAt => _createdAt;
        public virtual Guid? CreatedBy => _createdBy;
        public virtual DateTime? LastModifiedAt => _lastModifiedAt;
        public virtual Guid? LastModifiedBy => _lastModifiedBy;
        public virtual DateTime? DeletedAt => _deletedAt;
        public virtual Guid? DeletedBy => _deletedBy;
        public virtual bool IsDeleted => _isDeleted;

        public virtual void Create(Guid? createdBy = null)
        {
            _createdAt = DateTime.UtcNow;
            _createdBy = createdBy;
        }

        public virtual void Update(Guid? modifiedBy = null)
        {
            _lastModifiedAt = DateTime.UtcNow;
            _lastModifiedBy = modifiedBy;
        }

        public virtual void Delete(Guid? deletedBy = null)
        {
            if (!_isDeleted)
            {
                _isDeleted = true;
                _deletedAt = DateTime.UtcNow;
                _deletedBy = deletedBy;
            }
        }

        public virtual void Restore(Guid? restoredBy = null)
        {
            if (_isDeleted)
            {
                _isDeleted = false;
                _deletedAt = null;
                _deletedBy = null;
                Update(restoredBy);
            }
        }
    }
}