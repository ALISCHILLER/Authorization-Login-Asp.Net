namespace Authorization_Login_Asp.Net.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class LoginRequest
    {
        [Required(ErrorMessage = "وارد کردن نام کاربری الزامی است")]
        [StringLength(100, ErrorMessage = "نام کاربری نباید بیش از 100 کاراکتر باشد")]
        public string Username { get; set; }

        [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
