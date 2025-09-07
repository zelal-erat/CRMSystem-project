namespace CRMSystem.Application.DTOs.Service;

public class CreateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
