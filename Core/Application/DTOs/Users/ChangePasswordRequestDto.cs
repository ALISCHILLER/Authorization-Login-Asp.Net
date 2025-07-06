namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class ChangePasswordRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
