using CRMSystem.Application.Common;
using CRMSystem.Application.Common.Exceptions;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;

namespace CRMSystem.Application.Handlers.Customers;

public class CreateCustomerCommandHandler : IRequestHandler<CRMSystem.Application.Commands.Customers.CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICustomerDomainService _customerDomainService;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICustomerDomainService customerDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _customerDomainService = customerDomainService;
    }

    public async Task<Result<CustomerDto>> Handle(CRMSystem.Application.Commands.Customers.CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // AutoMapper ile DTO'yu Domain Request'e çevir
        var createRequest = _mapper.Map<CreateCustomerRequest>(request.Customer);

        var customerResult = await _customerDomainService.CreateCustomerAsync(createRequest);

        if (!customerResult.IsSuccess)
            return Result<CustomerDto>.Failure(customerResult.Error);

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customerResult.Data));
    }
}
