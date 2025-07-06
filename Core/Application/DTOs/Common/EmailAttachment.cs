namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = new byte[0];
        public string ContentType { get; set; } = string.Empty;
    }
}
