using CRMSystem.Application.Common;
using CRMSystem.Domain.Enums;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Services;
using CRMSystem.Application.DTOs.Dashboard;
using CRMSystem.Application.Queries.Dashboard;
using CRMSystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Application.Handlers.Dashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var dashboard = new DashboardDto();
        var today = DateTime.UtcNow.Date;

        // İstatistikler
        var stats = new DashboardStatsDto();

        // Toplam müşteri sayısı
        var customers = await _unitOfWork.Customers.GetAllAsync();
        stats.TotalCustomers = customers.Count();

        // Toplam fatura ve toplam gelir
        var invoices = await _unitOfWork.Invoices.GetInvoicesWithItemsAsync();

        stats.TotalInvoices = invoices.Count();

        // Toplam gelir: Sadece ödenmiş faturalar
        stats.TotalRevenue = invoices
            .Where(i => !i.IsDeleted && i.Status == InvoiceStatus.Paid)
            .SelectMany(i => i.Items.Where(ii => !ii.IsDeleted))
            .Sum(ii => ii.TotalAmount);

        // Toplam servis sayısı
        var services = await _unitOfWork.Services.GetActiveServicesAsync();
        stats.TotalServices = services.Count();

        // ✅ Optimizasyon: Repository metodlarını kullan (Database-side hesaplama)
        var overdueInvoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync();
        var upcomingInvoices = await _unitOfWork.Invoices.GetUpcomingInvoicesAsync();

        stats.OverdueInvoicesCount = overdueInvoices.Count();
        stats.UpcomingInvoicesCount = upcomingInvoices.Count();

        dashboard.Stats = stats;

        // Geciken faturalar detayı
        dashboard.OverdueInvoices = overdueInvoices
            .Select(invoice =>
            {
                var firstDueDate = invoice.Items.Where(i => !i.IsDeleted).Min(i => i.DueDate);
                return new OverdueInvoiceDto
                {
                    Id = invoice.Id,
                    CustomerName = invoice.Customer.FullName,
                    Description = invoice.Description,
                    TotalAmount = invoice.Items.Where(i => !i.IsDeleted).Sum(i => i.TotalAmount),
                    DueDate = firstDueDate,
                    DaysOverdue = (today - firstDueDate).Days
                };
            })
            .Take(10)
            .ToList();

        // Yaklaşan faturalar detayı
        dashboard.UpcomingInvoices = upcomingInvoices
            .Select(invoice =>
            {
                var firstDueDate = invoice.Items.Where(i => !i.IsDeleted).Min(i => i.DueDate);
                return new UpcomingInvoiceDto
                {
                    Id = invoice.Id,
                    CustomerName = invoice.Customer.FullName,
                    Description = invoice.Description,
                    TotalAmount = invoice.Items.Where(i => !i.IsDeleted).Sum(i => i.TotalAmount),
                    DueDate = firstDueDate,
                    DaysUntilDue = (firstDueDate - today).Days
                };
            })
            .Take(10)
            .ToList();

        // Gelir grafiği verileri
        var revenueChart = new RevenueChartDto();
        var monthlyData = new List<MonthlyRevenueDto>();

        for (int i = 5; i >= 0; i--)
        {
            var startDate = today.AddMonths(-i).AddDays(1 - today.AddMonths(-i).Day);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var monthlyInvoices = invoices
                .Where(inv => inv.CreatedAt >= startDate && inv.CreatedAt <= endDate)
                .ToList();

            monthlyData.Add(new MonthlyRevenueDto
            {
                Month = startDate.ToString("MMMM yyyy"),
                Revenue = monthlyInvoices
                    .Where(inv => !inv.IsDeleted && inv.Status == InvoiceStatus.Paid)
                    .SelectMany(inv => inv.Items.Where(ii => !ii.IsDeleted))
                    .Sum(ii => ii.TotalAmount),
                InvoiceCount = monthlyInvoices.Count
            });
        }

        revenueChart.MonthlyData = monthlyData;

        // Mevcut ay ve önceki ay gelirleri
        var currentMonthStart = today.AddDays(1 - today.Day);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart.AddDays(-1);

        var currentMonthInvoices = invoices
            .Where(inv => inv.CreatedAt >= currentMonthStart && inv.CreatedAt <= today)
            .ToList();

        var previousMonthInvoices = invoices
            .Where(inv => inv.CreatedAt >= previousMonthStart && inv.CreatedAt <= previousMonthEnd)
            .ToList();

        revenueChart.CurrentMonthRevenue = currentMonthInvoices
            .Where(inv => !inv.IsDeleted && inv.Status == InvoiceStatus.Paid)
            .SelectMany(inv => inv.Items.Where(ii => !ii.IsDeleted))
            .Sum(ii => ii.TotalAmount);

        revenueChart.PreviousMonthRevenue = previousMonthInvoices
            .Where(inv => !inv.IsDeleted && inv.Status == InvoiceStatus.Paid)
            .SelectMany(inv => inv.Items.Where(ii => !ii.IsDeleted))
            .Sum(ii => ii.TotalAmount);

        if (revenueChart.PreviousMonthRevenue > 0)
        {
            revenueChart.RevenueGrowth = ((revenueChart.CurrentMonthRevenue - revenueChart.PreviousMonthRevenue)
                / revenueChart.PreviousMonthRevenue) * 100;
        }

        dashboard.RevenueChart = revenueChart;

        return Result<DashboardDto>.Success(dashboard);
    }
}
