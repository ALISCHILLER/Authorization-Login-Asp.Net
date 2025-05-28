using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس بررسی سلامت سیستم
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// بررسی سلامت دیتابیس
        /// </summary>
        Task<bool> CheckDatabaseHealthAsync();

        /// <summary>
        /// بررسی سلامت سرویس‌های خارجی
        /// </summary>
        Task<bool> CheckExternalServicesHealthAsync();

        /// <summary>
        /// بررسی سلامت سیستم
        /// </summary>
        Task<bool> CheckSystemHealthAsync();

        /// <summary>
        /// دریافت وضعیت سلامت سیستم
        /// </summary>
        Task<HealthStatus> GetHealthStatusAsync();
    }

    /// <summary>
    /// وضعیت سلامت سیستم
    /// </summary>
    public class HealthStatus
    {
        /// <summary>
        /// وضعیت کلی
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// وضعیت دیتابیس
        /// </summary>
        public bool IsDatabaseHealthy { get; set; }

        /// <summary>
        /// وضعیت سرویس‌های خارجی
        /// </summary>
        public bool AreExternalServicesHealthy { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        public string ErrorMessage { get; set; }
    }
} 