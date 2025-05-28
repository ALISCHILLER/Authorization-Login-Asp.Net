namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// انواع روش‌های احراز هویت دو مرحله‌ای
    /// </summary>
    public enum TwoFactorType
    {
        /// <summary>
        /// احراز هویت از طریق اپلیکیشن
        /// </summary>
        App = 0,

        /// <summary>
        /// احراز هویت از طریق پیامک
        /// </summary>
        Sms = 1,

        /// <summary>
        /// احراز هویت از طریق ایمیل
        /// </summary>
        Email = 2
    }
} 