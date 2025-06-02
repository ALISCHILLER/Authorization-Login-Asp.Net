using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Events.Base;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.Account
{
    /// <summary>
    /// Event raised when a user's account is locked.
    /// This event tracks both automatic and manual account locks, including the reason and duration.
    /// </summary>
    public class AccountLockedEvent : AccountEvent
    {
        /// <summary>
        /// Gets the time when the account was locked.
        /// </summary>
        public DateTime LockedAt => EventTime;

        /// <summary>
        /// Gets the number of failed attempts that triggered the lock.
        /// </summary>
        public int FailedAttempts { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountLockedEvent"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user whose account was locked.</param>
        /// <param name="ipAddress">The IP address from which the lock was initiated.</param>
        /// <param name="userAgent">The user agent (browser/client) information.</param>
        /// <param name="lockDurationMinutes">The duration of the lock in minutes.</param>
        /// <param name="failedAttempts">The number of failed attempts that triggered the lock.</param>
        /// <param name="isAutomatic">Whether the lock was automatic.</param>
        /// <param name="reason">Optional reason for the account lock.</param>
        public AccountLockedEvent(
            Guid userId,
            string ipAddress,
            string userAgent,
            int lockDurationMinutes,
            int failedAttempts,
            bool isAutomatic = true,
            string reason = null)
            : base(userId, "Lock", ipAddress, userAgent, AccountStatus.Active, AccountStatus.Locked, lockDurationMinutes, isAutomatic, reason)
        {
            FailedAttempts = failedAttempts;
        }

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>A string containing the account lock details.</returns>
        public override string ToString()
        {
            return $"{base.ToString()} [Failed attempts: {FailedAttempts}]";
        }
    }
} 