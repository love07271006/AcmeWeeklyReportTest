using System.ComponentModel.DataAnnotations;

namespace AcmeWeeklyReportTest.Helpers
{
    public class HalfHourStepAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not decimal hours)
                return new ValidationResult("工作時間格式錯誤");

            // ✅ 允許 0
            if (hours == 0m)
                return ValidationResult.Success;

            // ❌ 小於 0.5
            if (hours < 0.5m)
                return new ValidationResult("工作時間最小為 0 或 0.5 小時");

            // ❌ 不是 0.5 的倍數
            if (hours % 0.5m != 0m)
                return new ValidationResult("工作時間必須以 0.5 小時為單位");

            return ValidationResult.Success;
        }
    }
}
