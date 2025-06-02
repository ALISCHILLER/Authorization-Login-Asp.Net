using System;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.User.Creation
{
    /// <summary>
    /// Event raised when a new user is created in the system.
    /// This event contains the initial user data and creation context.
    /// </summary>
    public class UserCreatedEvent : UserEvent
    {
        /// <summary>
        /// Gets the time when the user was created.
        /// </summary>
        public DateTime CreatedAt => EventTime;

        /// <summary>
        /// Gets the user entity that was created.
        /// This contains the complete user data at creation time.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets the email address of the created user.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the username of the created user.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCreatedEvent"/> class.
        /// </summary>
        /// <param name="user">The user entity that was created.</param>
        /// <param name="ipAddress">The IP address from which the user was created.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="reason">Optional reason for user creation.</param>
        /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
        public UserCreatedEvent(
            User user,
            string ipAddress = null,
            string userAgent = null,
            string reason = null)
            : base(user?.Id ?? throw new ArgumentNullException(nameof(user)), ipAddress, userAgent, reason)
        {
            User = user;
            Email = user.Email;
            Username = user.Username;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the event details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - User created: {Username} ({Email})" +
                   (!string.IsNullOrEmpty(Reason) ? $" ({Reason})" : "");
        }
    }
} 