using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// مدل درخواست اختصاص نقش به کاربر؛ این کلاس برای ارسال نام نقش (با اعتبارسنجی الزامی بودن) استفاده می‌شود.
    /// </summary>
    public class AssignRoleRequest
    {
        /// <summary>
        /// نام نقش (با اعتبارسنجی الزامی بودن)
        /// </summary>
        [Required(ErrorMessage = "نام نقش الزامی است")]
        public string RoleName { get; set; }
    }
}