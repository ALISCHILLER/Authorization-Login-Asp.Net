using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Base
{
    /// <summary>
    /// Base class for all authentication-related events.
    /// This class provides common properties and behavior for authentication events.
    /// </summary>
    public abstract class AuthenticationEvent : UserEvent
    {
        /// <summary>
        /// Gets the authentication type (e.g., "Login", "Logout", "Token").
        /// </summary>
        public string AuthenticationType { get; }

        /// <summary>
        /// Gets a value indicating whether the authentication was successful.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Gets the reason for authentication failure, if applicable.
        /// </summary>
        public string FailureReason { get; }

        /// <summary>
        /// Gets the session ID associated with this authentication event, if applicable.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Gets the expiration time of the authentication, if applicable.
        /// </summary>
        public DateTime? ExpiresAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="authenticationType">The type of authentication event.</param>
        /// <param name="isSuccessful">Whether the authentication was successful.</param>
        /// <param name="ipAddress">The IP address from which the authentication was attempted.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="sessionId">Optional session ID associated with this authentication.</param>
        /// <param name="expiresAt">Optional expiration time of the authentication.</param>
        /// <param name="failureReason">Optional reason for authentication failure.</param>
        protected AuthenticationEvent(
            Guid userId,
            string authenticationType,
            bool isSuccessful,
            string ipAddress,
            string userAgent,
            string sessionId = null,
            DateTime? expiresAt = null,
            string failureReason = null)
            : base(userId, ipAddress, userAgent, failureReason)
        {
            AuthenticationType = authenticationType ?? throw new ArgumentNullException(nameof(authenticationType));
            IsSuccessful = isSuccessful;
            SessionId = sessionId;
            ExpiresAt = expiresAt;
            FailureReason = failureReason;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the authentication event details.</returns>
        public override string ToString()
        {
            var result = $"{base.ToString()} - {AuthenticationType} {(IsSuccessful ? "successful" : "failed")}";
            
            if (!IsSuccessful && !string.IsNullOrEmpty(FailureReason))
                result += $" ({FailureReason})";
                
            if (!string.IsNullOrEmpty(SessionId))
                result += $" [Session: {SessionId}]";
                
            if (ExpiresAt.HasValue)
                result += $" (expires at {ExpiresAt.Value:yyyy-MM-dd HH:mm:ss})";
                
            return result;
        }
    }
} 