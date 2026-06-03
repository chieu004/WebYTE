using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Invoice;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto?> CreateInvoiceAsync(Guid appointmentId, decimal amount, decimal taxAmount = 0, string? notes = null)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return null;

        // Generate Invoice Number: INV-YYYYMMDD-XXXXX
        var today = DateTime.UtcNow;
        var invoiceCount = await _context.Invoices
            .Where(i => i.IssuedDate.Year == today.Year 
                && i.IssuedDate.Month == today.Month 
                && i.IssuedDate.Day == today.Day)
            .CountAsync();

        var invoiceNumber = $"INV-{today:yyyyMMdd}-{(invoiceCount + 1):D5}";

        var totalAmount = amount + taxAmount;

        var invoice = new Invoice
        {
            AppointmentId = appointmentId,
            InvoiceNumber = invoiceNumber,
            IssuedDate = today,
            DueDate = today.AddDays(30), // Hạn thanh toán 30 ngày
            Amount = amount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Created,
            Notes = notes
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return new InvoiceDto
        {
            Id = invoice.Id,
            AppointmentId = invoice.AppointmentId,
            InvoiceNumber = invoice.InvoiceNumber,
            IssuedDate = invoice.IssuedDate,
            DueDate = invoice.DueDate,
            PatientName = appointment.Patient.User.FullName,
            DoctorName = appointment.Doctor.User.FullName,
            Amount = invoice.Amount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            Notes = invoice.Notes
        };
    }

    public async Task<InvoiceDto?> GetInvoiceByAppointmentIdAsync(Guid appointmentId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Appointment)
            .ThenInclude(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
            .ThenInclude(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

        if (invoice == null)
            return null;

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Appointment)
            .ThenInclude(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
            .ThenInclude(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return null;

        return MapToDto(invoice);
    }

    public async Task<List<InvoiceDto>> GetInvoicesByPatientIdAsync(Guid patientId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Appointment)
            .ThenInclude(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
            .ThenInclude(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(i => i.Appointment.PatientId == patientId)
            .OrderByDescending(i => i.IssuedDate)
            .ToListAsync();

        return invoices.Select(MapToDto).ToList();
    }

    public async Task<bool> MarkAsSentAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
            return false;

        invoice.Status = InvoiceStatus.Sent;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<byte[]> ExportInvoicePdfAsync(Guid invoiceId)
    {
        // TODO: Implement PDF export using a library like iTextSharp or SelectPdf
        throw new NotImplementedException("PDF export will be implemented soon");
    }

    private InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            AppointmentId = invoice.AppointmentId,
            InvoiceNumber = invoice.InvoiceNumber,
            IssuedDate = invoice.IssuedDate,
            DueDate = invoice.DueDate,
            PatientName = invoice.Appointment.Patient.User.FullName,
            DoctorName = invoice.Appointment.Doctor.User.FullName,
            Amount = invoice.Amount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            Notes = invoice.Notes
        };
    }
}
