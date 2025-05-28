using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    public class NotificationRequest
    {
        [Required(ErrorMessage = "عنوان اعلان الزامی است")]
        [StringLength(100, ErrorMessage = "عنوان اعلان نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Title { get; set; }

        [Required(ErrorMessage = "متن اعلان الزامی است")]
        [StringLength(500, ErrorMessage = "متن اعلان نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Message { get; set; }

        [Required(ErrorMessage = "نوع اعلان الزامی است")]
        public NotificationType Type { get; set; }

        public string UserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
} 