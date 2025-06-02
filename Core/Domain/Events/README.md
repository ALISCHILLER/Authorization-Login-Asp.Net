# Domain Events Structure

This directory contains all domain events used in the application. Events are organized into the following categories:

## Directory Structure

```
Events/
├── Base/                    # Base classes and interfaces
│   ├── DomainEvent.cs       # Base domain event class
│   ├── UserEvent.cs         # Base user event class
│   ├── StatusChangeEvent.cs # Base status change event class
│   └── VerificationEvent.cs # Base verification event class
├── User/                    # User-related events
│   ├── Creation/           # User creation events
│   ├── Authentication/     # Authentication events
│   ├── Authorization/      # Authorization events
│   ├── Profile/           # Profile management events
│   └── Security/          # Security-related events
├── Account/                # Account-related events
├── Verification/          # Verification events
└── Common/                # Common/shared events
```

## Event Categories

### User Events
- Creation: Events related to user creation and deletion
- Authentication: Events related to login, logout, and authentication
- Authorization: Events related to permissions and roles
- Profile: Events related to profile updates
- Security: Events related to security measures (2FA, password changes, etc.)

### Account Events
- Status changes
- Locking/unlocking
- Account modifications

### Verification Events
- Email verification
- Phone verification
- Other verification types

### Common Events
- Shared events used across multiple domains
- Utility events

## Event Base Classes

1. `DomainEvent`: Base class for all domain events
   - Provides common properties: Id, OccurredOn, EventType
   - Implements IDomainEvent interface

2. `UserEvent`: Base class for user-related events
   - Extends DomainEvent
   - Adds user-specific properties: UserId, IpAddress, UserAgent

3. `StatusChangeEvent<T>`: Base class for status change events
   - Extends UserEvent
   - Generic type for different status types
   - Tracks old and new status values

4. `VerificationEvent`: Base class for verification events
   - Extends UserEvent
   - Handles verification-specific properties
   - Manages verification status and expiration

## Best Practices

1. Event Naming:
   - Use past tense for event names (e.g., UserCreated, PasswordChanged)
   - Be specific about what changed
   - Include the entity type in the name

2. Event Properties:
   - Include only necessary data
   - Make properties immutable
   - Use appropriate data types

3. Event Organization:
   - Place events in appropriate subdirectories
   - Follow consistent naming patterns
   - Group related events together

4. Event Handling:
   - Keep handlers focused and single-purpose
   - Use async/await for all operations
   - Handle errors appropriately 