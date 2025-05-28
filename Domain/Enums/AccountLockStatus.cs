namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// وضعیت قفل بودن حساب کاربری
    /// </summary>
    public enum AccountLockStatus
    {
        /// <summary>
        /// حساب کاربری باز است
        /// </summary>
        Unlocked = 0,

        /// <summary>
        /// حساب کاربری قفل شده است
        /// </summary>
        Locked = 1,

        /// <summary>
        /// حساب کاربری به صورت موقت قفل شده است
        /// </summary>
        TemporarilyLocked = 2
    }
} 