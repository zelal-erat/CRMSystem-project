namespace CRMSystem.Application.DTOs.Customer;

public class CustomerServiceUsageDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<ServiceUsageDetailDto> ServiceUsages { get; set; } = new();
    public decimal TotalSpent { get; set; }
    public int TotalServicesUsed { get; set; }
}

public class ServiceUsageDetailDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime LastUsedDate { get; set; }
}

public class CustomerServiceAnalysisDto
{
    public List<CustomerServiceUsageDto> CustomerUsages { get; set; } = new();
    public ServiceUsageSummaryDto Summary { get; set; } = new();
    public List<ServicePopularityDto> ServicePopularity { get; set; } = new();
}

public class ServiceUsageSummaryDto
{
    public int TotalCustomers { get; set; }
    public int TotalServices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageSpendingPerCustomer { get; set; }
}

public class ServicePopularityDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
}
