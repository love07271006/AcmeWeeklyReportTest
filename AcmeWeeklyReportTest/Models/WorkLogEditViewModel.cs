using System.ComponentModel.DataAnnotations;

namespace AcmeWeeklyReportTest.Models
{
    public class WorkLogEditViewModel
    {
        public int Sn { get; set; }

        [Display(Name = "開始日期")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "結束日期")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "工作事項")]
        [Required(ErrorMessage = "請輸入工作事項")]
        public string WorkItem { get; set; } = string.Empty;

        [Display(Name = "完成狀況")]
        public WorkStatus Status { get; set; }

        [Display(Name = "工作時間（小時）")]
        [Range(0, 999, ErrorMessage = "工作時間需為 0 以上")]
        public decimal WorkHours { get; set; }

        [Display(Name = "備註")]
        public string? Remark { get; set; }
    }
}
