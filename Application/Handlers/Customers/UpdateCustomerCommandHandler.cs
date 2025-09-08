using CRMSystem.Application.Common;
using CRMSystem.Application.Common.Exceptions;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Interfaces;
using CRMSystem.Domain.Services;
using MediatR;
using AutoMapper;
using CRMSystem.Application.Commands.Customers;

namespace CRMSystem.Application.Handlers.Customers;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICustomerDomainService _customerDomainService;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICustomerDomainService customerDomainService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _customerDomainService = customerDomainService;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        // AutoMapper ile DTO'yu Domain Request'e çevir
        var updateRequest = _mapper.Map<UpdateCustomerRequest>(request.Customer);

        var customerResult = await _customerDomainService.UpdateCustomerAsync(request.Id, updateRequest);

        if (!customerResult.IsSuccess)
            return Result<CustomerDto>.Failure(customerResult.Error);

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customerResult.Data));
    }
}