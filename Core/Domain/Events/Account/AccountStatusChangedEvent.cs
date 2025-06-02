using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Account
{
    /// <summary>
    /// Event raised when a user's account status changes.
    /// This event tracks transitions between different account states (e.g., Active, Suspended, Locked).
    /// </summary>
    public class AccountStatusChangedEvent : AccountEvent
    {
        /// <summary>
        /// Gets the time when the status change occurred.
        /// </summary>
        public DateTime StatusChangedAt => EventTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountStatusChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose account status changed.</param>
        /// <param name="oldStatus">The previous account status.</param>
        /// <param name="newStatus">The new account status.</param>
        /// <param name="ipAddress">The IP address from which the status change was initiated.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="isAutomatic">Whether the status change was automatic.</param>
        /// <param name="reason">Optional reason for the status change.</param>
        public AccountStatusChangedEvent(
            Guid userId,
            AccountStatus oldStatus,
            AccountStatus newStatus,
            string ipAddress = null,
            string userAgent = null,
            bool isAutomatic = false,
            string reason = null)
            : base(userId, "StatusChange", ipAddress, userAgent, oldStatus, newStatus, null, isAutomatic, reason)
        {
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the account status change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} for account";
        }
    }
} 