using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Authentication
{
    /// <summary>
    /// Event raised when a user logs out of the system.
    /// This event tracks both manual and automatic logouts.
    /// </summary>
    public class UserLoggedOutEvent : AuthenticationEvent
    {
        /// <summary>
        /// Gets the time when the logout occurred.
        /// </summary>
        public DateTime LogoutTime => EventTime;

        /// <summary>
        /// Gets a value indicating whether the logout was automatic (e.g., session timeout).
        /// </summary>
        public bool IsAutomatic { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoggedOutEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user who logged out.</param>
        /// <param name="ipAddress">The IP address from which the logout occurred.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="sessionId">The session ID that was logged out.</param>
        /// <param name="isAutomatic">Whether the logout was automatic.</param>
        /// <param name="reason">Optional reason for logout.</param>
        public UserLoggedOutEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            string sessionId,
            bool isAutomatic = false,
            string reason = null)
            : base(userId, "Logout", true, ipAddress, userAgent, sessionId, null, reason)
        {
            IsAutomatic = isAutomatic;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the logout details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} ({(IsAutomatic ? "automatic" : "manual")})";
        }
    }
} 