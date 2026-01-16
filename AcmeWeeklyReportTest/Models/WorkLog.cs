using System;
using System.Collections.Generic;

namespace AcmeWeeklyReportTest.Models;

public partial class WorkLog
{
    public int Sn { get; set; }

    public int UserSn { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string WorkItem { get; set; } = null!;

    public int Status { get; set; }

    public decimal WorkHours { get; set; }

    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User UserSnNavigation { get; set; } = null!;
}
