
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public abstract class DomainEventBase : IDomainEvent
    {
        protected DomainEventBase(Guid entityId, string entityType, Guid? userId = null)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EntityId = entityId;
            EntityType = entityType;
            UserId = userId;
            Version = 1;
            EventType = GetType().Name;
        }

        public Guid Id { get; }
        public DateTime OccurredOn { get; }
        public string EventType { get; }
        public Guid EntityId { get; }
        public string EntityType { get; }
        public Guid? UserId { get; }
        public int Version { get; }
        public IReadOnlyDictionary<string, object> Metadata => _metadata; // Implement interface property
        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>(); // Backing field

        // Optional: Method to add metadata
        public void AddMetadata(string key, object value)
        {
            _metadata[key] = value;
        }
    }
}
