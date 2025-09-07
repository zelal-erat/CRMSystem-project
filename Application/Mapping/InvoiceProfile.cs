using AutoMapper;
using CRMSystem.Application.DTOs.Invoice;
using CRMSystem.Domain.Entities;

namespace CRMSystem.Application.Mapping;

public class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
        // Entity -> DTO
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count(i => !i.IsDeleted)))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => 
                src.Items.Where(i => !i.IsDeleted).Min(i => i.DueDate)))
            .ForMember(dest => dest.IsRenewal, opt => opt.MapFrom(src => 
                src.Description.Contains("🔄 YENİLEME")))
            .ForMember(dest => dest.RenewalIndicator, opt => opt.MapFrom(src => 
                src.Description.Contains("🔄 YENİLEME") ? "🔄 YENİLEME" : ""));

        CreateMap<Invoice, InvoiceDetailDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsRenewal, opt => opt.MapFrom(src => 
                src.Description.Contains("🔄 YENİLEME")))
            .ForMember(dest => dest.RenewalIndicator, opt => opt.MapFrom(src => 
                src.Description.Contains("🔄 YENİLEME") ? "🔄 YENİLEME" : ""));

        CreateMap<InvoiceItem, InvoiceItemDto>()
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
            .ForMember(dest => dest.RenewalCycle, opt => opt.MapFrom(src => src.RenewalCycle.ToString()));

        // DTO -> Request mapping
        CreateMap<CreateInvoiceItemDto, CreateInvoiceItemRequest>()
            .ForMember(dest => dest.RenewalCycle, opt => opt.MapFrom(src => 
                Enum.Parse<CRMSystem.Domain.Enums.RenewalCycle>(src.RenewalCycle)));

        // DTO -> Entity (Create için)
        CreateMap<CreateInvoiceDto, Invoice>();
        CreateMap<CreateInvoiceItemDto, InvoiceItem>()
            .ForMember(dest => dest.RenewalCycle, opt => opt.MapFrom(src => 
                Enum.Parse<CRMSystem.Domain.Enums.RenewalCycle>(src.RenewalCycle)));

        // DTO -> Entity (Update için)
        CreateMap<UpdateInvoiceDto, Invoice>();
        CreateMap<UpdateInvoiceItemDto, InvoiceItem>()
            .ForMember(dest => dest.RenewalCycle, opt => opt.MapFrom(src => 
                Enum.Parse<CRMSystem.Domain.Enums.RenewalCycle>(src.RenewalCycle)));
    }
}
