using System;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Invoice;
using WebYTE.Core.Enums;

namespace WebYTE.Application.Interfaces;

public interface IInvoiceService
{
    Task<Guid> GenerateInvoiceAsync(Guid appointmentId);
    Task<InvoiceDto?> GetInvoiceDetailsAsync(Guid invoiceId);
    Task<bool> UpdateInvoiceStatusAsync(Guid invoiceId, InvoiceStatus status);
}
