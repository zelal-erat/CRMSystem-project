using AutoMapper;
using CRMSystem.Application.DTOs.Service;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Services;

namespace CRMSystem.Application.Mapping;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        // DTO -> Entity
        CreateMap<CreateServiceDto, Service>();
        CreateMap<UpdateServiceDto, Service>();

        // Entity -> DTO
        CreateMap<Service, ServiceDto>();
        
        // DTO -> Domain Request (Handler'ları sadeleştirmek için)
        CreateMap<CreateServiceDto, CreateServiceRequest>();
        CreateMap<UpdateServiceDto, UpdateServiceRequest>();
    }
}


