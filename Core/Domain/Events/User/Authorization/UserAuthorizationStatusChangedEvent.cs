using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Authorization
{
    /// <summary>
    /// Event raised when a user's authorization status changes for a specific resource or action.
    /// This event tracks both successful and failed authorization attempts.
    /// </summary>
    public class UserAuthorizationStatusChangedEvent : StatusChangeEvent<bool>
    {
        /// <summary>
        /// Gets the path of the resource being accessed.
        /// </summary>
        public string RequestPath { get; }

        /// <summary>
        /// Gets the HTTP method used in the request.
        /// </summary>
        public string RequestMethod { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthorizationStatusChangedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose authorization status changed.</param>
        /// <param name="wasAuthorized">Whether the user was previously authorized.</param>
        /// <param name="isAuthorized">Whether the user is now authorized.</param>
        /// <param name="ipAddress">The IP address from which the authorization was attempted.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="requestPath">The path of the resource being accessed.</param>
        /// <param name="requestMethod">The HTTP method used in the request.</param>
        /// <param name="reason">Optional reason for the authorization status change.</param>
        public UserAuthorizationStatusChangedEvent(
            Guid userId,
            bool wasAuthorized,
            bool isAuthorized,
            string ipAddress,
            string userAgent,
            string requestPath,
            string requestMethod,
            string reason = null)
            : base(userId, wasAuthorized, isAuthorized, ipAddress, userAgent, reason)
        {
            RequestPath = requestPath ?? throw new ArgumentNullException(nameof(requestPath));
            RequestMethod = requestMethod ?? throw new ArgumentNullException(nameof(requestMethod));
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the authorization status change details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} for authorization - {RequestMethod} {RequestPath}";
        }
    }
} 