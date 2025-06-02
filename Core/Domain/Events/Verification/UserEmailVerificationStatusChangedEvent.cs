using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Verification
{
    /// <summary>
    /// Event raised when a user's email verification status changes.
    /// This event tracks both successful and failed email verification attempts.
    /// </summary>
    public class UserEmailVerificationStatusChangedEvent : VerificationEvent
    {
        /// <summary>
        /// Gets the email address being verified.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEmailVerificationStatusChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose email verification status changed.</param>
        /// <param name="email">The email address being verified.</param>
        /// <param name="wasVerified">Whether the email was previously verified.</param>
        /// <param name="isVerified">Whether the email is now verified.</param>
        /// <param name="verificationId">Optional ID of the verification attempt.</param>
        /// <param name="expiresAt">Optional expiration time of the verification.</param>
        /// <param name="ipAddress">The IP address from which the verification was attempted.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="reason">Optional reason for the verification status change.</param>
        /// <exception cref="ArgumentNullException">Thrown when email is null or empty.</exception>
        public UserEmailVerificationStatusChangedEvent(
            Guid userId,
            string email,
            bool wasVerified,
            bool isVerified,
            string verificationId = null,
            DateTime? expiresAt = null,
            string ipAddress = null,
            string userAgent = null,
            string reason = null)
            : base(
                userId,
                verificationId ?? Guid.NewGuid().ToString(),
                "Email",
                wasVerified,
                isVerified,
                expiresAt,
                ipAddress,
                userAgent,
                reason)
        {
            Email = !string.IsNullOrEmpty(email) 
                ? email 
                : throw new ArgumentNullException(nameof(email));
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the email verification status change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} for email: {Email}";
        }
    }
} 