using System.ComponentModel;

namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// وضعیت‌های مختلف حساب کاربری
    /// این enum برای مدیریت وضعیت‌های مختلف حساب کاربری استفاده می‌شود
    /// </summary>
    public enum AccountStatus
    {
        /// <summary>
        /// حساب کاربری فعال و قابل استفاده است
        /// </summary>
        [Description("فعال")]
        Active = 1,

        /// <summary>
        /// حساب کاربری غیرفعال شده است (به صورت موقت)
        /// </summary>
        [Description("غیرفعال")]
        Inactive = 2,

        /// <summary>
        /// حساب کاربری به دلیل نقض قوانین یا درخواست مدیر مسدود شده است
        /// </summary>
        [Description("مسدود شده")]
        Blocked = 3,

        /// <summary>
        /// حساب کاربری در انتظار تأیید ایمیل است
        /// </summary>
        [Description("در انتظار تأیید ایمیل")]
        PendingEmailVerification = 4,

        /// <summary>
        /// حساب کاربری در انتظار تأیید شماره تلفن است
        /// </summary>
        [Description("در انتظار تأیید شماره تلفن")]
        PendingPhoneVerification = 5,

        /// <summary>
        /// حساب کاربری به دلیل تلاش‌های ناموفق متعدد قفل شده است
        /// </summary>
        [Description("قفل شده")]
        Locked = 6,

        /// <summary>
        /// حساب کاربری منقضی شده است (مثلاً به دلیل عدم استفاده طولانی مدت)
        /// </summary>
        [Description("منقضی شده")]
        Expired = 7,

        /// <summary>
        /// حساب کاربری حذف شده است (حذف منطقی)
        /// </summary>
        [Description("حذف شده")]
        Deleted = 8
    }
} 