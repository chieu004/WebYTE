using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using WebYTE.Core.Enums;

namespace WebYTE.Core.Entities;

public class ApplicationUser : IdentityUser<Guid> // Inherit with Guid as key
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public RoleType UserRole { get; set; }
    
    // Navigation properties for different user profiles
    public Patient? PatientProfile { get; set; }
    public Doctor? DoctorProfile { get; set; }
    public Staff? StaffProfile { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
