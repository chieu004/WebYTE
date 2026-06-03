using System;

namespace WebYTE.Application.DTOs.Invoice;

public class CreateInvoiceRequest
{
    public Guid AppointmentId { get; set; }
    public decimal Amount { get; set; } = 100000; // Mặc định 100.000 VND
    public decimal TaxAmount { get; set; } = 0;
    public string? Notes { get; set; }
}
