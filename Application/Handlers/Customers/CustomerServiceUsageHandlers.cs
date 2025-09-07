using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Application.Queries.Customers;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Enums;
using MediatR;

namespace CRMSystem.Application.Handlers.Customers;

public class GetCustomerServiceUsageQueryHandler : IRequestHandler<GetCustomerServiceUsageQuery, Result<CustomerServiceUsageDto?>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerServiceUsageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerServiceUsageDto?>> Handle(GetCustomerServiceUsageQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetCustomerWithServiceUsageAsync(request.CustomerId);
        
        if (customer == null)
            return Result<CustomerServiceUsageDto?>.Success(null);

        var serviceUsages = customer.Invoices
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid)
            .SelectMany(i => i.Items.Where(item => !item.IsDeleted))
            .GroupBy(item => item.ServiceId)
            .Select(group => new ServiceUsageDetailDto
            {
                ServiceId = group.Key,
                ServiceName = group.First().Service?.Name ?? "Bilinmeyen Hizmet",
                ServicePrice = group.First().Service?.Price ?? 0,
                UsageCount = group.Sum(item => item.Quantity),
                TotalAmount = group.Sum(item => item.TotalAmount),
                LastUsedDate = group.Max(item => item.StartDate)
            })
            .ToList();

        var totalSpent = serviceUsages.Sum(s => s.TotalAmount);

        var result = new CustomerServiceUsageDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.FullName,
            CustomerEmail = customer.Email,
            ServiceUsages = serviceUsages,
            TotalSpent = totalSpent,
            TotalServicesUsed = serviceUsages.Count
        };

        return Result<CustomerServiceUsageDto?>.Success(result);
    }
}

public class GetCustomerServiceAnalysisQueryHandler : IRequestHandler<GetCustomerServiceAnalysisQuery, Result<CustomerServiceAnalysisDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerServiceAnalysisQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerServiceAnalysisDto>> Handle(GetCustomerServiceAnalysisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var customers = await _unitOfWork.Customers.GetCustomersWithServiceUsageAsync();
            
            if (!request.IncludeInactiveCustomers)
            {
                customers = customers.Where(c => c.Invoices.Any(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid));
            }

            var customerUsages = new List<CustomerServiceUsageDto>();
            var servicePopularity = new Dictionary<int, ServicePopularityDto>();

            foreach (var customer in customers)
            {
                var serviceUsages = customer.Invoices
                    .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid)
                    .SelectMany(i => i.Items.Where(item => !item.IsDeleted))
                    .GroupBy(item => item.ServiceId)
                    .Select(g => new ServiceUsageDetailDto
                    {
                        ServiceId = g.Key,
                        ServiceName = g.First().Service?.Name ?? "Bilinmeyen Hizmet",
                        ServicePrice = g.First().Service?.Price ?? 0,
                        UsageCount = g.Sum(item => item.Quantity),
                        TotalAmount = g.Sum(item => item.TotalAmount),
                        LastUsedDate = g.Max(item => item.StartDate)
                    })
                    .ToList();

                var totalSpent = serviceUsages.Sum(s => s.TotalAmount);

                customerUsages.Add(new CustomerServiceUsageDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    CustomerEmail = customer.Email,
                    ServiceUsages = serviceUsages,
                    TotalSpent = totalSpent,
                    TotalServicesUsed = serviceUsages.Count
                });

                // Service popularity hesaplama
                foreach (var serviceUsage in serviceUsages)
                {
                    if (servicePopularity.ContainsKey(serviceUsage.ServiceId))
                    {
                        servicePopularity[serviceUsage.ServiceId].UsageCount += serviceUsage.UsageCount;
                        servicePopularity[serviceUsage.ServiceId].CustomerCount++;
                        servicePopularity[serviceUsage.ServiceId].TotalRevenue += serviceUsage.TotalAmount;
                    }
                    else
                    {
                        servicePopularity[serviceUsage.ServiceId] = new ServicePopularityDto
                        {
                            ServiceId = serviceUsage.ServiceId,
                            ServiceName = serviceUsage.ServiceName,
                            UsageCount = serviceUsage.UsageCount,
                            CustomerCount = 1,
                            TotalRevenue = serviceUsage.TotalAmount,
                            AveragePrice = serviceUsage.ServicePrice
                        };
                    }
                }
            }

            var summary = new ServiceUsageSummaryDto
            {
                TotalCustomers = customerUsages.Count,
                TotalServices = servicePopularity.Count,
                TotalRevenue = customerUsages.Sum(c => c.TotalSpent),
                AverageSpendingPerCustomer = customerUsages.Count > 0 ? (decimal)customerUsages.Average(c => c.TotalSpent) : 0
            };

            var analysis = new CustomerServiceAnalysisDto
            {
                CustomerUsages = customerUsages.OrderByDescending(c => c.TotalSpent).ToList(),
                Summary = summary,
                ServicePopularity = servicePopularity.Values.OrderByDescending(s => s.TotalRevenue).ToList()
            };

            return Result<CustomerServiceAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            return Result<CustomerServiceAnalysisDto>.Failure($"Müşteri hizmet analizi alınırken hata oluştu: {ex.Message}");
        }
    }
}

public class GetServiceUsageByCustomerQueryHandler : IRequestHandler<GetServiceUsageByCustomerQuery, Result<IEnumerable<CustomerServiceUsageDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetServiceUsageByCustomerQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<CustomerServiceUsageDto>>> Handle(GetServiceUsageByCustomerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.Customer> customers;

            if (request.ServiceId.HasValue)
            {
                customers = await _unitOfWork.Customers.GetCustomersByServiceIdAsync(request.ServiceId.Value);
            }
            else
            {
                customers = await _unitOfWork.Customers.GetCustomersWithServiceUsageAsync();
            }

            var result = customers.Select(customer =>
            {
                var serviceUsages = customer.Invoices
                    .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid)
                    .SelectMany(i => i.Items.Where(item => !item.IsDeleted))
                    .GroupBy(item => item.ServiceId)
                    .Select(group => new ServiceUsageDetailDto
                    {
                        ServiceId = group.Key,
                        ServiceName = group.First().Service?.Name ?? "Bilinmeyen Hizmet",
                        ServicePrice = group.First().Service?.Price ?? 0,
                        UsageCount = group.Sum(item => item.Quantity),
                        TotalAmount = group.Sum(item => item.TotalAmount),
                        LastUsedDate = group.Max(item => item.StartDate)
                    })
                    .ToList();

                var totalSpent = serviceUsages.Sum(s => s.TotalAmount);

                return new CustomerServiceUsageDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    CustomerEmail = customer.Email,
                    ServiceUsages = serviceUsages,
                    TotalSpent = totalSpent,
                    TotalServicesUsed = serviceUsages.Count
                };
            }).ToList();

            return Result<IEnumerable<CustomerServiceUsageDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CustomerServiceUsageDto>>.Failure($"Müşteri hizmet kullanımı alınırken hata oluştu: {ex.Message}");
        }
    }
}