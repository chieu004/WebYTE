using System;

namespace WebYTE.Core.Entities;

public class Staff : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public string? Position { get; set; }
    public string? Department { get; set; }
}
