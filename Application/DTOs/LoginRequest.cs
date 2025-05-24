namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// درخواست ورود به سیستم (Login)
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// ایمیل کاربر (برای احراز هویت)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// رمز عبور کاربر (برای احراز هویت)
        /// </summary>
        public string Password { get; set; }
    }
}
