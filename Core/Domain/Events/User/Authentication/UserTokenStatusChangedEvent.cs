using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Authentication
{
    /// <summary>
    /// Event raised when a user's token status changes (e.g., created, revoked, expired).
    /// This event tracks both successful and failed token operations.
    /// </summary>
    public class UserTokenStatusChangedEvent : AuthenticationEvent
    {
        /// <summary>
        /// Gets the type of token (e.g., "Access", "Refresh", "Reset").
        /// </summary>
        public string TokenType { get; }

        /// <summary>
        /// Gets the ID of the token.
        /// </summary>
        public string TokenId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTokenStatusChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose token status changed.</param>
        /// <param name="tokenType">The type of token.</param>
        /// <param name="tokenId">The ID of the token.</param>
        /// <param name="isSuccessful">Whether the token operation was successful.</param>
        /// <param name="ipAddress">The IP address from which the token operation was attempted.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="expiresAt">Optional expiration time of the token.</param>
        /// <param name="failureReason">Optional reason for token operation failure.</param>
        public UserTokenStatusChangedEvent(
            Guid userId,
            string tokenType,
            string tokenId,
            bool isSuccessful,
            string ipAddress,
            string userAgent,
            DateTime? expiresAt = null,
            string failureReason = null)
            : base(userId, $"Token_{tokenType}", isSuccessful, ipAddress, userAgent, tokenId, expiresAt, failureReason)
        {
            TokenType = tokenType ?? throw new ArgumentNullException(nameof(tokenType));
            TokenId = tokenId ?? throw new ArgumentNullException(nameof(tokenId));
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the token status change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - {TokenType} token {TokenId}";
        }
    }
} 