using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Verification
{
    /// <summary>
    /// Event raised when a user's phone verification status changes.
    /// This event tracks both successful and failed phone verification attempts.
    /// </summary>
    public class UserPhoneVerificationStatusChangedEvent : VerificationEvent
    {
        /// <summary>
        /// Gets the phone number being verified.
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPhoneVerificationStatusChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose phone verification status changed.</param>
        /// <param name="phoneNumber">The phone number being verified.</param>
        /// <param name="wasVerified">Whether the phone was previously verified.</param>
        /// <param name="isVerified">Whether the phone is now verified.</param>
        /// <param name="verificationId">Optional ID of the verification attempt.</param>
        /// <param name="expiresAt">Optional expiration time of the verification.</param>
        /// <param name="ipAddress">The IP address from which the verification was attempted.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="reason">Optional reason for the verification status change.</param>
        /// <exception cref="ArgumentNullException">Thrown when phoneNumber is null or empty.</exception>
        public UserPhoneVerificationStatusChangedEvent(
            Guid userId,
            string phoneNumber,
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
                "Phone",
                wasVerified,
                isVerified,
                expiresAt,
                ipAddress,
                userAgent,
                reason)
        {
            PhoneNumber = !string.IsNullOrEmpty(phoneNumber) 
                ? phoneNumber 
                : throw new ArgumentNullException(nameof(phoneNumber));
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the phone verification status change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} for phone: {PhoneNumber}";
        }
    }
} 