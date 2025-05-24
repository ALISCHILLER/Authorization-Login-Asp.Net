namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// نوع یا دسته‌بندی پرمیشن‌ها برای سازماندهی بهتر
    /// مثلا: پرمیشن‌های مرتبط با کاربر، محتوا، گزارش و ...
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// مدیریت کاربران
        /// </summary>
        UserManagement,

        /// <summary>
        /// مدیریت محتوا
        /// </summary>
        ContentManagement,

        /// <summary>
        /// مدیریت گزارش‌ها
        /// </summary>
        Reporting,

        /// <summary>
        /// تنظیمات سیستم
        /// </summary>
        Settings
    }
}
