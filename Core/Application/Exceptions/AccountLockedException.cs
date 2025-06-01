using System;

namespace Authorization_Login_Asp.Net.Core.Application.Exceptions
{
    public class AccountLockedException : Exception
    {
        public AccountLockedException() : base("حساب کاربری قفل شده است")
        {
        }

        public AccountLockedException(string message) : base(message)
        {
        }

        public AccountLockedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DateTime? LockoutEnd { get; set; }
        public int RemainingMinutes { get; set; }
    }
}