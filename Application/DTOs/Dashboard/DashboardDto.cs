namespace CRMSystem.Application.DTOs.Dashboard;

public class DashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<OverdueInvoiceDto> OverdueInvoices { get; set; } = new();
    public List<UpcomingInvoiceDto> UpcomingInvoices { get; set; } = new();
    public RevenueChartDto RevenueChart { get; set; } = new();
}

public class DashboardStatsDto
{
    public int TotalCustomers { get; set; }
    public int TotalInvoices { get; set; }
    public int TotalServices { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OverdueInvoicesCount { get; set; }
    public int UpcomingInvoicesCount { get; set; }
}

public class OverdueInvoiceDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
}

public class UpcomingInvoiceDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysUntilDue { get; set; }
}

public class RevenueChartDto
{
    public List<MonthlyRevenueDto> MonthlyData { get; set; } = new();
    public decimal CurrentMonthRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int InvoiceCount { get; set; }
}
