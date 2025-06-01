namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// تعریف نقش‌های کاربری در سیستم
    /// این Enum مشخص می‌کند که یک کاربر چه نقش یا دسترسی اصلی دارد.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// مدیر کل سیستم
        /// </summary>
        Admin,

        /// <summary>
        /// کاربر عادی با دسترسی محدود
        /// </summary>
        User,

        /// <summary>
        /// مدیر بخش یا اپراتور با دسترسی‌های خاص
        /// </summary>
        Manager,

        /// <summary>
        /// کاربر مهمان یا بازدیدکننده
        /// </summary>
        Guest
    }
}
