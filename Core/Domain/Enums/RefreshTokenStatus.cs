namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// وضعیت توکن رفرش برای مدیریت اعتبار و انقضا
    /// این enum برای کنترل وضعیت توکن‌های رفرش استفاده می‌شود
    /// </summary>
    public enum RefreshTokenStatus
    {
        /// <summary>
        /// توکن فعال و قابل استفاده است
        /// </summary>
        Active,         // فعال و قابل استفاده

        /// <summary>
        /// توکن لغو شده است (مثلاً در صورت سوءاستفاده یا درخواست کاربر)
        /// </summary>
        Revoked,        // لغو شده (مثلاً در صورت سوءاستفاده)

        /// <summary>
        /// توکن منقضی شده و دیگر قابل استفاده نیست
        /// </summary>
        Expired         // منقضی شده
    }
}