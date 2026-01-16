using System;
using System.Collections.Generic;

namespace AcmeWeeklyReportTest.Models;

public partial class User
{
    public int Sn { get; set; }

    public string Account { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
}
