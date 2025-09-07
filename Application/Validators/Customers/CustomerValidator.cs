using FluentValidation;
using CRMSystem.Application.DTOs.Customer;
using CRMSystem.Application.Commands.Customers;

namespace CRMSystem.Application.Validators.Customers;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad Soyad alanı boş olamaz.")
            .MaximumLength(200).WithMessage("Ad Soyad 200 karakterden uzun olamaz.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(100).WithMessage("E-posta 100 karakterden uzun olamaz.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon 20 karakterden uzun olamaz.");

        RuleFor(x => x.TaxOffice)
            .MaximumLength(100).WithMessage("Vergi Dairesi 100 karakterden uzun olamaz.");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(20).WithMessage("Vergi Numarası 20 karakterden uzun olamaz.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres 500 karakterden uzun olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakterden uzun olamaz.");
    }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir ID gerekli.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad Soyad alanı boş olamaz.")
            .MaximumLength(200).WithMessage("Ad Soyad 200 karakterden uzun olamaz.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(100).WithMessage("E-posta 100 karakterden uzun olamaz.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon 20 karakterden uzun olamaz.");

        RuleFor(x => x.TaxOffice)
            .MaximumLength(100).WithMessage("Vergi Dairesi 100 karakterden uzun olamaz.");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(20).WithMessage("Vergi Numarası 20 karakterden uzun olamaz.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres 500 karakterden uzun olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama 1000 karakterden uzun olamaz.");
    }
}


