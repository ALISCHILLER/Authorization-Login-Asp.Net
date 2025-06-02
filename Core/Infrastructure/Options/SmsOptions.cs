namespace Authorization_Login_Asp.Net.Core.Infrastructure.Options
{
    public class SmsOptions
    {
        public bool UseDevelopmentMode { get; set; }
        public string ApiKey { get; set; }
        public string SenderId { get; set; }
        public string ApiUrl { get; set; }
    }
} 