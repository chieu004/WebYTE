using System;

namespace WebYTE.Core.Entities;

public class Medication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Usage { get; set; }
    public string? SideEffects { get; set; }
    public string Unit { get; set; } = string.Empty; // e.g., Viên, Vỉ, Hộp
    public decimal Price { get; set; }
    public int InStock { get; set; }
}
