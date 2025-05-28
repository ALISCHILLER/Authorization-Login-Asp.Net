namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// مدل انتقال داده کاربر
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// شناسه یکتا
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// نقش
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا ایمیل تأیید شده است
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// آخرین زمان ورود
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
