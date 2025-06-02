using System.ComponentModel;

namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// وضعیت‌های مختلف توکن بازنشانی
    /// این enum برای مدیریت وضعیت توکن‌های بازنشانی استفاده می‌شود
    /// </summary>
    public enum RefreshTokenStatus
    {
        /// <summary>
        /// توکن فعال و قابل استفاده است
        /// </summary>
        [Description("فعال")]
        Active = 1,

        /// <summary>
        /// توکن لغو شده است (به صورت دستی یا خودکار)
        /// </summary>
        [Description("لغو شده")]
        Revoked = 2,

        /// <summary>
        /// توکن منقضی شده است
        /// </summary>
        [Description("منقضی شده")]
        Expired = 3
    }
}