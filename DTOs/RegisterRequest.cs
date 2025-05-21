namespace Authorization_Login_Asp.Net.DTOs
{
    using Authorization_Login_Asp.Net.model;
    using System.ComponentModel.DataAnnotations;

    public class RegisterRequest
    {
        [Required(ErrorMessage = "وارد کردن نام کاربری الزامی است")]
        [StringLength(100, ErrorMessage = "نام کاربری نباید بیش از 100 کاراکتر باشد")]
        public string Username { get; set; }

        [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "وارد کردن نقش کاربر الزامی است")]
        public UserRole Role { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل وارد شده معتبر نیست")]
        public string Email { get; set; }
    }
}
