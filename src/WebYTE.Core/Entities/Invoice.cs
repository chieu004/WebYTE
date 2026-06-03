using System;
using WebYTE.Core.Enums;

namespace WebYTE.Core.Entities;

public class Invoice : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    
    public string InvoiceNumber { get; set; } = string.Empty; // Format: INV-YYYYMMDD-XXXXX
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    
    public decimal Amount { get; set; } = 0; // Phí khám bệnh
    public decimal TaxAmount { get; set; } = 0; // Thuế (nếu có)
    public decimal TotalAmount { get; set; } = 0; // Tổng tiền
    
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Created;
    
    public string? Notes { get; set; }
}
