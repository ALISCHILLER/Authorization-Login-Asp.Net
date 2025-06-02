using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Authentication
{
    /// <summary>
    /// Event raised when a user attempts to log in to the system.
    /// This event tracks both successful and failed login attempts.
    /// </summary>
    public class UserLoggedInEvent : AuthenticationEvent
    {
        /// <summary>
        /// Gets the time when the login attempt occurred.
        /// </summary>
        public DateTime LoginTime => EventTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoggedInEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user attempting to log in.</param>
        /// <param name="ipAddress">The IP address from which the login attempt was made.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="isSuccessful">Whether the login attempt was successful.</param>
        /// <param name="sessionId">Optional session ID for the login.</param>
        /// <param name="expiresAt">Optional expiration time of the login session.</param>
        /// <param name="failureReason">Optional reason for login failure.</param>
        public UserLoggedInEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            bool isSuccessful = true,
            string sessionId = null,
            DateTime? expiresAt = null,
            string failureReason = null)
            : base(userId, "Login", isSuccessful, ipAddress, userAgent, sessionId, expiresAt, failureReason)
        {
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the login attempt details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - Login {(IsSuccessful ? "successful" : "failed")}" +
                   (!IsSuccessful && !string.IsNullOrEmpty(FailureReason) ? $" ({FailureReason})" : "");
        }
    }
} 