using System;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    public class AccountLockStatus
    {
        private const int MaxFailedAttempts = 5;

        public static readonly AccountLockStatus Locked = new AccountLockStatus
        {
            IsLocked = true,
            LockoutEnd = DateTime.UtcNow.AddMinutes(30),
            FailedAttempts = MaxFailedAttempts
        };

        public static readonly AccountLockStatus Unlocked = new AccountLockStatus
        {
            IsLocked = false,
            LockoutEnd = null,
            FailedAttempts = 0
        };

        public bool IsLocked { get; private set; }
        public DateTime? LockoutEnd { get; private set; }
        public int FailedAttempts { get; private set; }
        public int RemainingAttempts => MaxFailedAttempts - FailedAttempts;

        private AccountLockStatus()
        {
        }

        public static AccountLockStatus Create()
        {
            return new AccountLockStatus
            {
                IsLocked = false,
                LockoutEnd = null,
                FailedAttempts = 0
            };
        }

        public void IncrementFailedAttempts()
        {
            FailedAttempts++;
            if (FailedAttempts >= MaxFailedAttempts)
            {
                Lock();
            }
        }

        public void Lock()
        {
            IsLocked = true;
            LockoutEnd = DateTime.UtcNow.AddMinutes(30);
        }

        public void Reset()
        {
            IsLocked = false;
            LockoutEnd = null;
            FailedAttempts = 0;
        }

        public bool ShouldUnlock()
        {
            return IsLocked && LockoutEnd.HasValue && LockoutEnd.Value <= DateTime.UtcNow;
        }
    }
}