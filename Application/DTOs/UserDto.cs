namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// اطلاعات خلاصه کاربر برای نمایش یا بازگشت در API
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}
