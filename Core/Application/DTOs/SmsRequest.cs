using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    public class SmsRequest
    {
        public string To { get; set; }
        public string Message { get; set; }
        public NotificationPriority Priority { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public SmsRequest()
        {
            Priority = NotificationPriority.Normal;
            Metadata = new Dictionary<string, string>();
        }

        public SmsRequest(string to, string message)
            : this()
        {
            To = to;
            Message = message;
        }

        public SmsRequest(string to, string message, NotificationPriority priority)
            : this(to, message)
        {
            Priority = priority;
        }

        public SmsRequest(string to, string message, NotificationPriority priority, Dictionary<string, string> metadata)
            : this(to, message, priority)
        {
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
} 