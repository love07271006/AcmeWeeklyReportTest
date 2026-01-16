using System.ComponentModel.DataAnnotations;
using AcmeWeeklyReportTest.Helpers;

namespace AcmeWeeklyReportTest.Models
{
    public class WorkLogCreateViewModel : IValidatableObject
    {
        [Display(Name = "開始日期")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "結束日期")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "工作事項")]
        [Required(ErrorMessage = "請輸入工作事項")]
        [StringLength(500, ErrorMessage = "工作事項最多 500 字")]
        public string WorkItem { get; set; } = string.Empty;

        [Display(Name = "完成狀況")]
        public WorkStatus Status { get; set; } = WorkStatus.未開始;

        [Display(Name = "工作時間（小時）")]
        [Range(0, 999, ErrorMessage = "工作時間需為 0 以上")]
        [HalfHourStep]
        public decimal WorkHours { get; set; } = 0;

        [Display(Name = "備註")]
        [StringLength(1000, ErrorMessage = "備註最多 1000 字")]
        public string? Remark { get; set; }

        // ✅ 後端跨欄位驗證
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //日期卡控
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "結束日期不可早於開始日期",
                    new[] { nameof(EndDate) }
                );
            }
            //工作時數卡控
            if (WorkHours <= 0 && Status != WorkStatus.未開始)
            {
                yield return new ValidationResult(
                    "工作時間必須大於 0",
                    new[] { nameof(WorkHours) }
                );
            }
        }
    }

    public enum WorkStatus
    {
        未開始 = 0,
        進行中 = 1,
        待處理 = 2,
        已完成 = 3
    }
}
