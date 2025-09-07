using AutoMapper;
using CRMSystem.Application.DTOs.Service;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Application.Mapping;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        // DTO -> Entity
        CreateMap<CreateServiceDto, Service>();

        // Entity -> DTO
        CreateMap<Service, ServiceDto>();
    }
}


