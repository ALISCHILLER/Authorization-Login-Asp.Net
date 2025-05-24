namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// درخواست تمدید توکن (Refresh Token)
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// توکن دسترسی منقضی‌شده (JWT Token)
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن رفرش معتبر (برای دریافت توکن جدید)
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
