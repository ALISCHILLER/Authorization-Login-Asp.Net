namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string TwoFactorCode { get; set; }
        public string RecoveryCode { get; set; }
    }
}