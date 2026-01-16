using System.ComponentModel.DataAnnotations;

namespace AcmeWeeklyReportTest.Models
{
    public class LoginViewModel
    {
        [Display(Name = "帳號")]
        [Required(ErrorMessage = "請輸入帳號")]
        public string Account { get; set; } = string.Empty;

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }
    }
}
