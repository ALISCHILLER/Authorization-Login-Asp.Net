namespace Authorization_Login_Asp.Net.Core.Infrastructure.Options
{
    public class EmailOptions
    {
        public bool UseDevelopmentMode { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
} 