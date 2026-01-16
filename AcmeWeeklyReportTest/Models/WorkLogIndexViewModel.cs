using System.ComponentModel.DataAnnotations;

namespace AcmeWeeklyReportTest.Models
{
    public class WorkLogIndexViewModel
    {
        // 查詢條件
        [Display(Name = "開始日期(起)")]
        [DataType(DataType.Date)]
        public DateTime? StartFrom { get; set; }

        [Display(Name = "開始日期(迄)")]
        [DataType(DataType.Date)]
        public DateTime? StartTo { get; set; }

        [Display(Name = "狀態")]
        public WorkStatus? Status { get; set; }

        [Display(Name = "關鍵字")]
        public string? Keyword { get; set; }

        // 查詢結果
        public List<WorkLog> Items { get; set; } = new();
    }
}
