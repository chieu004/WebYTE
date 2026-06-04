using System;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Invoice;

namespace WebYTE.Application.Interfaces;

public interface IInvoiceService
{
    Task<Guid> GenerateInvoiceAsync(Guid appointmentId);
    Task<InvoiceDto?> GetInvoiceDetailsAsync(Guid invoiceId);
}
