using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public NotificationPriority Priority { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public EmailRequest()
        {
            Priority = NotificationPriority.Normal;
            Metadata = new Dictionary<string, string>();
        }

        public EmailRequest(string to, string subject, string body)
            : this()
        {
            To = to;
            Subject = subject;
            Body = body;
        }

        public EmailRequest(string to, string subject, string body, NotificationPriority priority)
            : this(to, subject, body)
        {
            Priority = priority;
        }

        public EmailRequest(string to, string subject, string body, NotificationPriority priority, Dictionary<string, string> metadata)
            : this(to, subject, body, priority)
        {
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
} 