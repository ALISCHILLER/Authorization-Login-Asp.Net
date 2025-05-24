namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// وضعیت توکن رفرش برای مدیریت اعتبار و انقضا
    /// </summary>
    public enum RefreshTokenStatus
    {
        Active,         // فعال و قابل استفاده
        Revoked,        // لغو شده (مثلاً در صورت سوءاستفاده)
        Expired         // منقضی شده
    }
}