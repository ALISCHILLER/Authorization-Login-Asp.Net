using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Authentication
{
    /// <summary>
    /// Event raised when a user's session status changes (e.g., created, expired, terminated).
    /// This event tracks both successful and failed session operations.
    /// </summary>
    public class UserSessionStatusChangedEvent : AuthenticationEvent
    {
        /// <summary>
        /// Gets the ID of the session.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSessionStatusChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose session status changed.</param>
        /// <param name="sessionId">The ID of the session.</param>
        /// <param name="isSuccessful">Whether the session operation was successful.</param>
        /// <param name="ipAddress">The IP address from which the session operation was attempted.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="expiresAt">Optional expiration time of the session.</param>
        /// <param name="failureReason">Optional reason for session operation failure.</param>
        public UserSessionStatusChangedEvent(
            Guid userId,
            string sessionId,
            bool isSuccessful,
            string ipAddress,
            string userAgent,
            DateTime? expiresAt = null,
            string failureReason = null)
            : base(userId, "Session", isSuccessful, ipAddress, userAgent, sessionId, expiresAt, failureReason)
        {
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the session status change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - Session {SessionId}";
        }
    }
} 