using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Security
{
    /// <summary>
    /// Event raised when a user's password is changed or reset.
    /// This event tracks both user-initiated changes and system-initiated resets.
    /// </summary>
    public class PasswordChangedEvent : UserEvent
    {
        /// <summary>
        /// Gets the time when the password was changed.
        /// </summary>
        public DateTime ChangedAt => EventTime;

        /// <summary>
        /// Gets a value indicating whether the change was initiated by the user.
        /// </summary>
        public bool IsChangedByUser { get; }

        /// <summary>
        /// Gets a value indicating whether this was a password reset operation.
        /// </summary>
        public bool IsPasswordReset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose password was changed.</param>
        /// <param name="ipAddress">The IP address from which the change was made.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="isChangedByUser">Whether the change was initiated by the user.</param>
        /// <param name="isPasswordReset">Whether this was a password reset operation.</param>
        /// <param name="reason">Optional reason for the password change.</param>
        public PasswordChangedEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            bool isChangedByUser = true,
            bool isPasswordReset = false,
            string reason = null)
            : base(userId, ipAddress, userAgent, reason)
        {
            IsChangedByUser = isChangedByUser;
            IsPasswordReset = isPasswordReset;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the password change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - Password {(IsPasswordReset ? "reset" : "changed")}" +
                   $" by {(IsChangedByUser ? "user" : "system")}" +
                   (!string.IsNullOrEmpty(Reason) ? $" ({Reason})" : "");
        }
    }
} 