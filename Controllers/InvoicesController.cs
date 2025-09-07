using CRMSystem.Application.Commands.Invoice;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Application.Queries.Invoice;
using CRMSystem.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CRMSystem.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IInvoiceDomainService _invoiceDomainService;

    public InvoicesController(IMediator mediator, IInvoiceDomainService invoiceDomainService)
    {
        _mediator = mediator;
        _invoiceDomainService = invoiceDomainService;
    }

    // ✅ Tüm faturaları listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetInvoicesQuery());
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    // ✅ Id ile fatura getir
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        if (result.Data is null)
            return NotFound(new { Success = false, Message = "Fatura bulunamadı" });

        return Ok(new { Success = true, Data = result.Data });
    }

    // ✅ Yeni fatura oluştur
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
    {
        var result = await _mediator.Send(new CreateInvoiceCommand(dto));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, new { Success = true, Data = result.Data });
    }

    // ✅ Faturayı güncelle
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateInvoiceDto dto)
    {
        var result = await _mediator.Send(new UpdateInvoiceCommand(dto));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    // ✅ Fatura sil
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteInvoiceCommand(id));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Message = "Fatura başarıyla silindi" });
    }

    // ✅ Yaklaşan faturalar (bu ay)
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var result = await _mediator.Send(new GetUpcomingInvoicesQuery());
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    // ✅ Gecikmiş faturalar
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue()
    {
        var result = await _mediator.Send(new GetOverdueInvoicesQuery());
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    // ✅ Faturayı ödenmiş işaretle
    [HttpPut("{id:int}/mark-paid")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var result = await _mediator.Send(new MarkInvoiceAsPaidCommand(id));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Message = "Fatura ödenmiş olarak işaretlendi" });
    }


    // ✅ Servis fiyatını getir (Business Rule: Hizmet seçildiğinde fiyat otomatik gelsin)
    [HttpGet("service-price/{serviceId:int}")]
    public async Task<IActionResult> GetServicePrice(int serviceId)
    {
        var result = await _invoiceDomainService.GetServicePriceAsync(serviceId);
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = new { Price = result.Data } });
    }

    // ✅ Yenileme faturalarını manuel olarak işle
    [HttpPost("process-renewals")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRenewals()
    {
        var result = await _invoiceDomainService.GenerateRecurringInvoicesAsync();
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Message = "Yenileme faturaları başarıyla işlendi" });
    }
}
