using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Invoice;

namespace WebYTE.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> CreateInvoiceAsync(Guid appointmentId, decimal amount, decimal taxAmount = 0, string? notes = null);
    Task<InvoiceDto?> GetInvoiceByAppointmentIdAsync(Guid appointmentId);
    Task<List<InvoiceDto>> GetInvoicesByPatientIdAsync(Guid patientId);
    Task<bool> MarkAsSentAsync(Guid invoiceId);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId);
    Task<byte[]> ExportInvoicePdfAsync(Guid invoiceId);
}
