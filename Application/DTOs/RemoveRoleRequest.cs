using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// مدل درخواست حذف نقش از کاربر؛ این کلاس برای ارسال نام نقش (با اعتبارسنجی الزامی بودن) استفاده می‌شود.
    /// </summary>
    public class RemoveRoleRequest
    {
        /// <summary>
        /// نام نقش (با اعتبارسنجی الزامی بودن)
        /// </summary>
        [Required(ErrorMessage = "نام نقش الزامی است")]
        public string RoleName { get; set; }
    }
} 