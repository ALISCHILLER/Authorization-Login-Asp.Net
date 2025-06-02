using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Account
{
    /// <summary>
    /// Event raised when a user's account is unlocked.
    /// This event tracks both automatic unlocks (after lock duration expires) and manual unlocks.
    /// </summary>
    public class AccountUnlockedEvent : AccountEvent
    {
        /// <summary>
        /// Gets the time when the account was unlocked.
        /// </summary>
        public DateTime UnlockedAt => EventTime;

        /// <summary>
        /// Gets a value indicating whether the unlock was automatic (e.g., after lock duration expired).
        /// </summary>
        public bool IsAutomatic { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountUnlockedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose account was unlocked.</param>
        /// <param name="ipAddress">The IP address from which the unlock was initiated.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="isAutomatic">Whether the unlock was automatic.</param>
        /// <param name="reason">Optional reason for the account unlock.</param>
        public AccountUnlockedEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            bool isAutomatic = false,
            string reason = null)
            : base(userId, "Unlock", ipAddress, userAgent, AccountStatus.Locked, AccountStatus.Active, null, isAutomatic, reason)
        {
            IsAutomatic = isAutomatic;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the account unlock details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - Account unlocked {(IsAutomatic ? "automatically" : "manually")}" +
                   (!string.IsNullOrEmpty(Reason) ? $" ({Reason})" : "");
        }
    }
} 