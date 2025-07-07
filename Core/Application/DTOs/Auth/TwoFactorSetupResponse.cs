namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class TwoFactorSetupResponse
    {
        public string Secret { get; set; } = string.Empty;
        public byte[] QrCodeImage { get; set; } = Array.Empty<byte>();
        public string ManualEntryKey { get; set; } = string.Empty;
    }
}
