using System;

namespace Authorization_Login_Asp.Net.Infrastructure.Configurations
{
    public class JaegerSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6831;
        public string Protocol { get; set; } = "udp";
    }
} 