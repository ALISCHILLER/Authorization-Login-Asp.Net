using System;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Profile
{
    /// <summary>
    /// Event raised when a user's profile information is updated.
    /// This event tracks which fields were changed and who made the changes.
    /// </summary>
    public class UserProfileUpdatedEvent : UserEvent
    {
        /// <summary>
        /// Gets the time when the profile was updated.
        /// </summary>
        public DateTime UpdatedAt => EventTime;

        /// <summary>
        /// Gets the list of fields that were changed in the profile.
        /// </summary>
        public string[] ChangedFields { get; }

        /// <summary>
        /// Gets a value indicating whether the changes were made by the user themselves.
        /// </summary>
        public bool IsChangedByUser { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileUpdatedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose profile was updated.</param>
        /// <param name="ipAddress">The IP address from which the update was made.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="changedFields">The list of fields that were changed.</param>
        /// <param name="isChangedByUser">Whether the changes were made by the user themselves.</param>
        /// <param name="reason">Optional reason for the profile update.</param>
        /// <exception cref="ArgumentNullException">Thrown when changedFields is null.</exception>
        public UserProfileUpdatedEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            string[] changedFields,
            bool isChangedByUser = true,
            string reason = null)
            : base(userId, ipAddress, userAgent, reason)
        {
            ChangedFields = changedFields ?? throw new ArgumentNullException(nameof(changedFields));
            IsChangedByUser = isChangedByUser;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the profile update details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - Profile updated by {(IsChangedByUser ? "user" : "system")}" +
                   $" - Changed fields: {string.Join(", ", ChangedFields)}" +
                   (!string.IsNullOrEmpty(Reason) ? $" ({Reason})" : "");
        }
    }
} 