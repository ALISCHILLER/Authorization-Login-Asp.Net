namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// انواع روش‌های احراز هویت دو مرحله‌ای
    /// </summary>
    public enum TwoFactorType
    {
        /// <summary>
        /// ایمیل
        /// </summary>
        Email = 0,

        /// <summary>
        /// پیامک
        /// </summary>
        Sms = 1,

        /// <summary>
        /// اپلیکیشن
        /// </summary>
        App = 2
    }
}