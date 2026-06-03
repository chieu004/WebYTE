using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Invoice;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public DateTime IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
}
