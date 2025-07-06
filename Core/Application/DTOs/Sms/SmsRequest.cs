using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Sms
{
    public class SmsRequest
    {
        public string To { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> TemplateData { get; set; }
        public string TemplateId { get; set; }
        public bool IsFlash { get; set; }
    }
} 