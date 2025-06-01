namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// انواع دسترسی‌ها (پرمیشن‌ها) در سیستم
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// دسترسی به خواندن اطلاعات
        /// </summary>
        Read = 1,

        /// <summary>
        /// دسترسی به ایجاد اطلاعات جدید
        /// </summary>
        Create = 2,

        /// <summary>
        /// دسترسی به ویرایش اطلاعات
        /// </summary>
        Update = 3,

        /// <summary>
        /// دسترسی به حذف اطلاعات
        /// </summary>
        Delete = 4,

        /// <summary>
        /// دسترسی به مدیریت کاربران
        /// </summary>
        ManageUsers = 5,

        /// <summary>
        /// دسترسی به مدیریت نقش‌ها
        /// </summary>
        ManageRoles = 6,

        /// <summary>
        /// دسترسی به مدیریت دسترسی‌ها
        /// </summary>
        ManagePermissions = 7,

        /// <summary>
        /// دسترسی به تنظیمات سیستم
        /// </summary>
        ManageSettings = 8,

        /// <summary>
        /// دسترسی به گزارش‌گیری
        /// </summary>
        GenerateReports = 9,

        /// <summary>
        /// دسترسی به مدیریت لاگ‌ها
        /// </summary>
        ManageLogs = 10
    }
}
