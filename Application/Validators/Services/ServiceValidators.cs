using FluentValidation;
using CRMSystem.Application.DTOs.Service;

namespace CRMSystem.Application.Validators.Services;

public class CreateServiceValidator : AbstractValidator<CreateServiceDto>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Servis adı zorunludur.")
            .MaximumLength(200).WithMessage("Servis adı 200 karakteri aşamaz.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama 500 karakteri aşamaz.");
    }
}

public class UpdateServiceValidator : AbstractValidator<UpdateServiceDto>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir ID gerekli.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Servis adı zorunludur.")
            .MaximumLength(200).WithMessage("Servis adı 200 karakteri aşamaz.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama 500 karakteri aşamaz.");
    }
}


