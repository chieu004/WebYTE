using System;

namespace WebYTE.Application.DTOs.Medication;

public class MedicationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Usage { get; set; }
    public string? SideEffects { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InStock { get; set; }
}
