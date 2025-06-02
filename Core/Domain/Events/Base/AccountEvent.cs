using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Base
{
    /// <summary>
    /// Base class for all account-related events.
    /// This class provides common properties and behavior for account events.
    /// </summary>
    public abstract class AccountEvent : UserEvent
    {
        /// <summary>
        /// Gets the type of account event (e.g., "StatusChange", "Lock", "Unlock").
        /// </summary>
        public string AccountEventType { get; }

        /// <summary>
        /// Gets the previous account status, if applicable.
        /// </summary>
        public AccountStatus? PreviousStatus { get; }

        /// <summary>
        /// Gets the new account status, if applicable.
        /// </summary>
        public AccountStatus? NewStatus { get; }

        /// <summary>
        /// Gets the duration of the account action in minutes, if applicable.
        /// </summary>
        public int? DurationMinutes { get; }

        /// <summary>
        /// Gets a value indicating whether the account action was automatic.
        /// </summary>
        public bool IsAutomatic { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="accountEventType">The type of account event.</param>
        /// <param name="ipAddress">The IP address from which the account action was initiated.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="previousStatus">Optional previous account status.</param>
        /// <param name="newStatus">Optional new account status.</param>
        /// <param name="durationMinutes">Optional duration of the account action in minutes.</param>
        /// <param name="isAutomatic">Whether the account action was automatic.</param>
        /// <param name="reason">Optional reason for the account action.</param>
        protected AccountEvent(
            Guid userId,
            string accountEventType,
            string ipAddress,
            string userAgent,
            AccountStatus? previousStatus = null,
            AccountStatus? newStatus = null,
            int? durationMinutes = null,
            bool isAutomatic = false,
            string reason = null)
            : base(userId, ipAddress, userAgent, reason)
        {
            AccountEventType = accountEventType ?? throw new ArgumentNullException(nameof(accountEventType));
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            DurationMinutes = durationMinutes;
            IsAutomatic = isAutomatic;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the account event details.</returns>
        public override string ToString()
        {
            var result = $"{base.ToString()} - {AccountEventType}";
            
            if (PreviousStatus.HasValue && NewStatus.HasValue)
                result += $" ({PreviousStatus.Value} -> {NewStatus.Value})";
            else if (NewStatus.HasValue)
                result += $" ({NewStatus.Value})";
                
            if (DurationMinutes.HasValue)
                result += $" [Duration: {DurationMinutes.Value} minutes]";
                
            if (IsAutomatic)
                result += " (automatic)";
                
            if (!string.IsNullOrEmpty(Reason))
                result += $" ({Reason})";
                
            return result;
        }
    }
} 