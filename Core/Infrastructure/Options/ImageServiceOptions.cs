namespace Authorization_Login_Asp.Net.Core.Infrastructure.Options
{
    /// <summary>
    /// تنظیمات سرویس تصویر
    /// </summary>
    public class ImageServiceOptions
    {
        /// <summary>
        /// مسیر ذخیره تصاویر
        /// </summary>
        public string UploadPath { get; set; } = "uploads";

        /// <summary>
        /// حداکثر اندازه فایل (به بایت)
        /// </summary>
        public int MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// حداکثر عرض تصویر
        /// </summary>
        public int MaxWidth { get; set; } = 800;

        /// <summary>
        /// حداکثر ارتفاع تصویر
        /// </summary>
        public int MaxHeight { get; set; } = 600;
    }
}