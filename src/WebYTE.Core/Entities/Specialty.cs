using System;
using System.Collections.Generic;

namespace WebYTE.Core.Entities;

public class Specialty : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
