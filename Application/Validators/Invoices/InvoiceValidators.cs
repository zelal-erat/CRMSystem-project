using FluentValidation;
using CRMSystem.Application.DTOs.Invoice;

namespace CRMSystem.Application.Validators.Invoices;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Geçerli bir müşteri ID'si gerekli.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakteri aşamaz.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("En az bir fatura kalemi gerekli.")
            .Must(items => items.Count > 0).WithMessage("En az bir fatura kalemi gerekli.");

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemValidator());
    }
}

public class CreateInvoiceItemValidator : AbstractValidator<CreateInvoiceItemDto>
{
    public CreateInvoiceItemValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("Geçerli bir servis ID'si gerekli.");

        RuleFor(x => x.RenewalCycle)
            .NotEmpty().WithMessage("Yenileme döngüsü gerekli.")
            .Must(cycle => cycle == "Monthly" || cycle == "Yearly")
            .WithMessage("Yenileme döngüsü 'Monthly' veya 'Yearly' olmalı.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalı.");

        RuleFor(x => x.VAT)
            .GreaterThanOrEqualTo(0).WithMessage("KDV negatif olamaz.")
            .LessThanOrEqualTo(100).WithMessage("KDV %100'ü aşamaz.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi gerekli.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakteri aşamaz.");
    }
}

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir ID gerekli.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Geçerli bir müşteri ID'si gerekli.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakteri aşamaz.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Fatura durumu gerekli.")
            .Must(status => status == "Draft" || status == "Pending" || status == "Issued" || 
                           status == "Paid" || status == "Overdue" || status == "Cancelled")
            .WithMessage("Geçerli bir fatura durumu giriniz.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("En az bir fatura kalemi gerekli.")
            .Must(items => items.Count > 0).WithMessage("En az bir fatura kalemi gerekli.");

        RuleForEach(x => x.Items).SetValidator(new UpdateInvoiceItemValidator());
    }
}

public class UpdateInvoiceItemValidator : AbstractValidator<UpdateInvoiceItemDto>
{
    public UpdateInvoiceItemValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir ID gerekli.");

        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("Geçerli bir servis ID'si gerekli.");

        RuleFor(x => x.RenewalCycle)
            .NotEmpty().WithMessage("Yenileme döngüsü gerekli.")
            .Must(cycle => cycle == "Monthly" || cycle == "Yearly")
            .WithMessage("Yenileme döngüsü 'Monthly' veya 'Yearly' olmalı.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalı.");

        RuleFor(x => x.VAT)
            .GreaterThanOrEqualTo(0).WithMessage("KDV negatif olamaz.")
            .LessThanOrEqualTo(100).WithMessage("KDV %100'ü aşamaz.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi gerekli.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakteri aşamaz.");
    }
}
