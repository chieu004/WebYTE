using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebYTE.Application.DTOs.Invoice;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public InvoiceService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Guid> GenerateInvoiceAsync(Guid appointmentId)
    {
        // Lấy thông tin appointment với tất cả các thông tin liên quan
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .Include(a => a.MedicalRecord)
                .ThenInclude(mr => mr!.Prescription)
                    .ThenInclude(p => p!.Details)
                        .ThenInclude(pd => pd.Medication)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            throw new Exception("Không tìm thấy cuộc hẹn");

        // Kiểm tra xem đã có invoice chưa
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

        if (existingInvoice != null)
            return existingInvoice.Id; // Trả về invoice đã có

        // Tính toán chi phí
        decimal consultationFee = appointment.Doctor.ConsultationFee;
        decimal medicationTotal = 0;

        // Tính tổng tiền thuốc nếu có đơn thuốc
        if (appointment.MedicalRecord?.Prescription != null)
        {
            medicationTotal = appointment.MedicalRecord.Prescription.Details
                .Sum(pd => pd.Quantity * pd.Medication.Price);
        }

        decimal totalAmount = consultationFee + medicationTotal;
        decimal taxAmount = totalAmount * 0.08m; // VAT 8%
        decimal finalAmount = totalAmount + taxAmount;

        // Tạo invoice
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            InvoiceNumber = GenerateInvoiceNumber(),
            IssuedDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(7), // 7 ngày
            Amount = totalAmount,
            TaxAmount = taxAmount,
            TotalAmount = finalAmount,
            Status = InvoiceStatus.Created,
            CreatedAt = DateTime.Now
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice.Id;
    }

    public async Task<InvoiceDto?> GetInvoiceDetailsAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.Specialty)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.MedicalRecord)
                    .ThenInclude(mr => mr!.Prescription)
                        .ThenInclude(p => p!.Details)
                            .ThenInclude(pd => pd.Medication)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return null;

        var appointment = invoice.Appointment;
        var patient = appointment.Patient;
        var doctor = appointment.Doctor;

        var dto = new InvoiceDto
        {
            Id = invoice.Id,
            AppointmentId = invoice.AppointmentId,
            InvoiceNumber = invoice.InvoiceNumber,
            IssuedDate = invoice.IssuedDate,
            DueDate = invoice.DueDate,

            // Thông tin bệnh nhân
            PatientName = patient.User.FullName,
            PatientPhone = patient.User.PhoneNumber ?? "",
            PatientAddress = patient.User.Address ?? "",

            // Thông tin bác sĩ
            DoctorName = doctor.User.FullName,
            SpecialtyName = doctor.Specialty?.Name ?? "",

            // Thông tin cuộc hẹn
            AppointmentDate = appointment.AppointmentDate,
            Reason = appointment.Reason ?? "",
            Diagnosis = appointment.MedicalRecord?.Diagnosis ?? "",

            // Chi phí
            ConsultationFee = doctor.ConsultationFee,
            MedicationTotal = 0,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,

            Status = invoice.Status,
            Notes = invoice.Notes,
            
            // Thông tin ngân hàng từ appsettings
            BankingInfo = new BankingInfoDto
            {
                BankId = _configuration["BankingInfo:BankId"] ?? "970436",
                BankName = _configuration["BankingInfo:BankName"] ?? "Vietcombank",
                AccountNo = _configuration["BankingInfo:AccountNo"] ?? "1234567890",
                AccountName = _configuration["BankingInfo:AccountName"] ?? "PHONG KHAM WEBYTE"
            }
        };

        // Thêm thông tin thuốc nếu có
        if (appointment.MedicalRecord?.Prescription != null)
        {
            foreach (var detail in appointment.MedicalRecord.Prescription.Details)
            {
                var medDto = new InvoiceMedicationDto
                {
                    MedicationName = detail.Medication.Name,
                    Dosage = detail.Dosage,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.Medication.Price,
                    TotalPrice = detail.Quantity * detail.Medication.Price,
                    Instructions = detail.Instructions
                };

                dto.Medications.Add(medDto);
                dto.MedicationTotal += medDto.TotalPrice;
            }
        }

        return dto;
    }

    private string GenerateInvoiceNumber()
    {
        // Format: INV-YYYYMMDD-XXXXX
        var date = DateTime.Now.ToString("yyyyMMdd");
        var random = new Random().Next(10000, 99999);
        return $"INV-{date}-{random}";
    }
}
