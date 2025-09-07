using CRMSystem.Application.Common;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Application.Commands.Customers;
using CRMSystem.Application.Queries.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CRMSystem.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetCustomersQuery(searchTerm, pageNumber, pageSize));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        if (result.Data is null)
            return NotFound(new { Success = false, Message = "Müşteri bulunamadı" });

        return Ok(new { Success = true, Data = result.Data });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(dto));

        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, new { Success = true, Data = result.Data });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCustomerDto dto)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(id, dto));

        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(id));
        return Ok(result);
    }

    // Müşteri bazlı hizmet kullanım raporları
    [HttpGet("{id:int}/service-usage")]
    public async Task<IActionResult> GetCustomerServiceUsage([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetCustomerServiceUsageQuery { CustomerId = id });
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        if (result.Data is null)
            return NotFound(new { Success = false, Message = "Müşteri bulunamadı" });

        return Ok(new { Success = true, Data = result.Data });
    }

    [HttpGet("service-analysis")]
    public async Task<IActionResult> GetCustomerServiceAnalysis([FromQuery] bool includeInactiveCustomers = false)
    {
        var result = await _mediator.Send(new GetCustomerServiceAnalysisQuery 
        { 
            IncludeInactiveCustomers = includeInactiveCustomers
        });
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }

    [HttpGet("service-usage-by-customer")]
    public async Task<IActionResult> GetServiceUsageByCustomer([FromQuery] int? serviceId)
    {
        var result = await _mediator.Send(new GetServiceUsageByCustomerQuery
        {
            ServiceId = serviceId
        });
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }
}
