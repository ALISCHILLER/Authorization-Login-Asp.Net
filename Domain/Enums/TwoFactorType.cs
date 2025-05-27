namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// انواع روش‌های احراز هویت دو مرحله‌ای
    /// </summary>
    public enum TwoFactorType
    {
        /// <summary>
        /// احراز هویت با اپلیکیشن (مثل Google Authenticator)
        /// </summary>
        AuthenticatorApp = 1,

        /// <summary>
        /// احراز هویت با پیامک
        /// </summary>
        Sms = 2,

        /// <summary>
        /// احراز هویت با ایمیل
        /// </summary>
        Email = 3
    }
} 