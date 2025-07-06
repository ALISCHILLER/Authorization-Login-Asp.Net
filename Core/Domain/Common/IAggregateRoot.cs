using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Events;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public interface IAggregateRoot : IEntity
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
}