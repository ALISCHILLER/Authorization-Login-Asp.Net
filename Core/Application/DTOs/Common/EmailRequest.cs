using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    public class EmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
        public List<EmailAttachment> Attachments { get; set; } = new();
    }
}